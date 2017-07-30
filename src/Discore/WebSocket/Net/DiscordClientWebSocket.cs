using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    /**
     * REUSABILITY:
     * Any object inheriting this class CANNOT be reused. Once one connection attempt has
     * taken place, a new instance will need to be created to try again.
     * 
     * This is due to the fact that System.Net.WebSockets.ClientWebSocket will never re-enter
     * the 'None' state, which is the only valid state for starting a connection.
     * 
     * CANCELLATION TOKENS:
     * Cancelling any token passed to this class will ABORT the entire socket and cancel
     * all pending IO operations.
    **/

    abstract class DiscordClientWebSocket : IDisposable
    {
        /// <summary>
        /// Custom error (not specified by Discord or WebSocket spec) to use when the client needs to disconnect
        /// due to an error on our end.
        /// </summary>
        public const WebSocketCloseStatus INTERNAL_CLIENT_ERROR = (WebSocketCloseStatus)4100; // TODO: check for side-effects of usage

        /// <summary>
        /// Gets whether the socket is currently connected.
        /// </summary>
        public virtual bool IsConnected => State == WebSocketState.Open;

        /// <summary>
        /// Gets whether the socket is in a state that can be disconnected.
        /// </summary>
        public virtual bool CanBeDisconnected => State == WebSocketState.Open
            || State == WebSocketState.CloseSent
            || State == WebSocketState.CloseReceived;

        protected WebSocketState State => socket.State;

        const int SEND_BUFFER_SIZE = 4 * 1024;  // 4kb (Discord's max payload size)
        const int RECEIVE_BUFFER_SIZE = 12 * 1024; // 12kb

        ClientWebSocket socket;

        CancellationTokenSource abortCancellationSource;
        Task receiveTask;

        AsyncLock sendLock;

        DiscoreLogger log;

        bool isDisposed;

        protected DiscordClientWebSocket(string loggingName)
        {
            log = new DiscoreLogger($"BaseWebSocket:{loggingName}");

            socket = new ClientWebSocket();
            // Disable "keep alive" packets, Discord's WebSocket servers do not handle
            // these and will disconnect us with an 'decode error'.
            socket.Options.KeepAliveInterval = TimeSpan.Zero;
        }

        /// <summary>
        /// Called when a payload has been received successfully.
        /// </summary>
        protected abstract Task OnPayloadReceived(DiscordApiData payload);
        /// <summary>
        /// Called when a close message has been received. 
        /// The socket will be gracefully closed automatically before this call.
        /// </summary>
        protected abstract void OnCloseReceived(WebSocketCloseStatus closeStatus, string closeDescription); // Successful close
        /// <summary>
        /// Called when either the socket closes or the receive task ends unexpectedly.
        /// The socket may or may not be open when this is called.
        /// </summary>
        protected abstract void OnClosedPrematurely(); // Unsuccessful close

        /// <param name="cancellationToken">Token that when cancelled will abort the entire socket.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="uri"/> does not start with ws:// or wss://.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="uri"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the socket attempts to start after a first time. A WebSocket instance
        /// can only be used for one connection attempt.
        /// </exception>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="ObjectDisposedException">Thrown if this socket has already been disposed.</exception>
        /// <exception cref="WebSocketException">Thrown if the socket fails to connect.</exception>
        public virtual async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            // Attempt to connect
            log.LogVerbose($"Connecting to {uri}...");
            await socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);

            // Shouldn't ever happen, but just in case
            if (socket.State != WebSocketState.Open)
            {
                log.LogWarning($"Socket.ConnectAsync succeeded but the state is {socket.State}!");
                throw new WebSocketException(WebSocketError.Faulted, "Failed to connect. No other information is available.");
            }

            // Connection successful (exception would be thrown otherwise)
            abortCancellationSource = new CancellationTokenSource();

            sendLock = new AsyncLock();

            // Start receive task
            receiveTask = ReceiveLoop();

            log.LogVerbose("Successfully connected.");
        }

        /// <summary>
        /// This task will deadlock (for 5s then abort the socket) if called from the same thread as the receive loop!
        /// </summary>
        /// <param name="cancellationToken">Token that when cancelled will abort the entire socket.</param>
        /// <exception cref="OperationCanceledException"></exception>
        /// <exception cref="WebSocketException">Thrown if the socket is not in a valid state to be closed.</exception>
        public virtual Task DisconnectAsync(WebSocketCloseStatus closeStatus, string statusDescription,
            CancellationToken cancellationToken)
        {
            // Since this operation can take up to 5s and is synchronous code, wrap it in a task.
            return Task.Run(() =>
            {
                log.LogVerbose($"Disconnecting with code {closeStatus} ({(int)closeStatus})...");

                // Since the state check will be eaten by the code below,
                // and we cannot reliably check for this below, check if
                // the socket state is valid for a close, and throw the
                // same exception CloseAsync would have if not valid.
                if (socket.State != WebSocketState.Open && socket.State != WebSocketState.CloseReceived
                    && socket.State != WebSocketState.CloseSent)
                    throw new WebSocketException(WebSocketError.InvalidState, $"Socket is in an invalid state: {socket.State}");

                // CloseAsync will send the close message and then wait until
                // a close frame has been received. Once nothing is holding
                // the receive lock, CloseAsync will then try and receive
                // data until it comes to a close frame, or another receive
                // call gets it.
                Task closeTask = socket.CloseAsync(closeStatus, statusDescription, cancellationToken);

                try
                {
                    // Give the socket 5s to gracefully disconnect.
                    if (!Task.WaitAll(new Task[] { closeTask, receiveTask }, 5000, cancellationToken))
                    {
                        // Socket did not gracefully disconnect in the given time,
                        // so abort the socket and move on.
                        log.LogWarning("Socket failed to disconnect after 5s, aborting...");
                        abortCancellationSource.Cancel();
                    }
                }
                catch (OperationCanceledException)
                {
                    // Caller cancelled, socket has been aborted.
                    log.LogVerbose("Disconnect cancelled by caller, socket has been aborted.");
                    throw;
                }
                catch (AggregateException aex)
                {
                    foreach (Exception ex in aex.InnerExceptions)
                    {
                        if (ex is WebSocketException wsex)
                        {
                            if (wsex.WebSocketErrorCode == WebSocketError.InvalidState)
                                // The socket being aborted is normal.
                                continue;
                            else
                                log.LogError($"Uncaught exception found while disconnecting: code = {wsex.WebSocketErrorCode}, error = {ex}");
                        }
                        else
                            log.LogError($"Uncaught exception found while disconnecting: {ex}");
                    }
                }

                log.LogVerbose("Disconnected.");
            });
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        /// <exception cref="JsonWriterException">Thrown if the given data cannot be serialized as JSON.</exception>
        protected async Task SendAsync(DiscordApiData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (socket.State != WebSocketState.Open)
                throw new InvalidOperationException("Cannot send data when the socket is not open.");

            // Serialize the data as JSON and convert to bytes
            byte[] bytes = Encoding.UTF8.GetBytes(data.SerializeToJson());

            // Wait for any existing send operations,
            // ClientWebSocket only supports one send operation at a time.
            using (await sendLock.LockAsync().ConfigureAwait(false))
            {
                // Now that we have acquired the lock, check if the socket is still open.
                // If not, just ignore this message so we can effectively cancel any pending sends after close.
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await SendData(bytes).ConfigureAwait(false);
                    }
                    catch (InvalidOperationException iex) // also catches ObjectDisposedException
                    {
                        throw new DiscordWebSocketException("The WebSocket connection was closed while sending data.", 
                            DiscordWebSocketError.ConnectionClosed, iex);
                    }
                    catch (OperationCanceledException ocex)
                    {
                        throw new DiscordWebSocketException("The WebSocket connection was aborted while sending data.",
                                DiscordWebSocketError.ConnectionClosed, ocex);
                    }
                    catch (WebSocketException wsex)
                    {
                        if (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                            throw new DiscordWebSocketException("The WebSocket connection was closed while sending data.",
                                DiscordWebSocketError.ConnectionClosed, wsex);
                        else
                        {
                            // The only known WebSocketException is ConnectionClosedPrematurely, so
                            // log any others to be handled later.
                            log.LogError($"[SendAsync] Unexpected WebSocketException error: code = {wsex.WebSocketErrorCode}, error = {wsex}");

                            // Should never happen
                            throw new DiscordWebSocketException("An unexpected WebSocket error occured while sending data.",
                                DiscordWebSocketError.Unexpected, wsex);
                        }
                    }
                    catch (Exception ex)
                    {
                        // We are not expecting any other exceptions, but since the exceptions thrown from
                        // System.Net.WebSockets.ClientWebSocket.SendAsync is not documented, log any
                        // unknown exceptions so we can handle them later.
                        log.LogError($"[SendAsync] Unhandled exception: {ex}");

                        // Should never happen, but we should consolidate everything into DiscordWebSocketException
                        // so handling exceptions we do not know about is at least consistent.
                        throw new DiscordWebSocketException("An unexpected error occured while sending data.",
                            DiscordWebSocketError.Unexpected, ex);
                    }
                }
            }
        }

        /// <exception cref="InvalidOperationException">Thrown if socket is not connected.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the socket is disposed before or while sending.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the socket is aborted while sending.</exception>
        /// <exception cref="WebSocketException">Thrown if the underlying socket stream is closed.</exception>
        async Task SendData(byte[] data)
        {
            int byteCount = data.Length;
            int frameCount = (int)Math.Ceiling((double)byteCount / SEND_BUFFER_SIZE);

            int offset = 0;
            for (int i = 0; i < frameCount; i++, offset += SEND_BUFFER_SIZE)
            {
                bool isLast = i == (frameCount - 1);

                int count;
                if (isLast)
                    count = byteCount - (i * SEND_BUFFER_SIZE);
                else
                    count = SEND_BUFFER_SIZE;

                ArraySegment<byte> arraySeg = new ArraySegment<byte>(data, offset, count);

                // Can only throw:
                //   OperationCanceledException (if aborted)
                //   WebSocketException (if stream closed)
                //   ObjectDisposedException (if disposed)
                //   InvalidOperationException (if not connected)
                await socket.SendAsync(arraySeg, WebSocketMessageType.Text, isLast, abortCancellationSource.Token)
                    .ConfigureAwait(false);
            }
        }

        async Task ReceiveLoop()
        {
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[RECEIVE_BUFFER_SIZE]);

            using (MemoryStream ms = new MemoryStream())
            {
                WebSocketReceiveResult result = null;
                bool isClosing = false;

                try
                {
                    while (socket.State == WebSocketState.Open)
                    {
                        // Reset memory stream for next message
                        ms.Position = 0;
                        ms.SetLength(0);

                        // Continue receiving data until a full message is read or the socket is no longer open.
                        do
                        {
                            try
                            {
                                // This call only throws WebSocketExceptions.
                                result = await socket.ReceiveAsync(buffer, abortCancellationSource.Token).ConfigureAwait(false);
                            }
                            catch (WebSocketException wsex)
                            {
                                // Only two errors here should be InvalidState (if socket is aborted),
                                // or ConnectionClosedPrematurely (with an inner exception detailing what happened).

                                if (wsex.WebSocketErrorCode == WebSocketError.InvalidState)
                                    log.LogVerbose($"[ReceiveLoop] Socket was aborted while receiving message. code = {wsex.WebSocketErrorCode}");
                                else if (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
                                {
                                    log.LogError($"[ReceiveLoop] Socket closed prematurely while receiving: code = {wsex.WebSocketErrorCode}");

                                    // Notify inherting object
                                    OnClosedPrematurely();
                                }
                                else
                                {
                                    log.LogError("[ReceiveLoop] Socket encountered error while receiving: " +
                                        $"code = {wsex.WebSocketErrorCode}, error = {wsex}");

                                    // Notify inherting object
                                    OnClosedPrematurely();
                                }

                                break;
                            }

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                // Server is disconnecting us
                                isClosing = true;

                                log.LogVerbose($"[ReceiveLoop] Received close: {result.CloseStatusDescription} " +
                                    $"{result.CloseStatus} ({(int)result.CloseStatus})");

                                try
                                {
                                    log.LogVerbose("[ReceiveLoop] Completing close handshake with status NormalClosure (1000)...");

                                    // Complete the closing handshake
                                    // TODO: Check that a 'Normal closure' here won't ALWAYS force us to create a new gateway session,
                                    // it is only documented that this happens when we INITIATE the closing handshake.
                                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", abortCancellationSource.Token)
                                        .ConfigureAwait(false);

                                    log.LogVerbose("[ReceiveLoop] Completed close handshake.");
                                }
                                catch (Exception ex)
                                {
                                    log.LogError($"[ReceiveLoop] Failed to complete closing handshake: {ex}");
                                }

                                // Notify inheriting object
                                OnCloseReceived(result.CloseStatus.Value, result.CloseStatusDescription);
                                break;
                            }
                            else
                                // Data message, append buffer to memory stream
                                ms.Write(buffer.Array, 0, result.Count);
                        }
                        while (socket.State == WebSocketState.Open && !result.EndOfMessage);

                        if (isClosing || socket.State == WebSocketState.Aborted)
                            break;

                        // Parse the message
                        string message = null;

                        try
                        {
                            message = await ParseMessage(result.MessageType, ms).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            log.LogError($"[ReceiveLoop] Failed to parse message: {ex}");
                        }

                        if (message != null)
                        {
                            if (DiscordApiData.TryParseJson(message, out DiscordApiData data))
                            {
                                try
                                {
                                    // Notify inheriting object that a payload has been received.
                                    await OnPayloadReceived(data);
                                }
                                // Payload handlers can send other payloads which can result in two
                                // valid exceptions that we do not want to bubble up.
                                catch (InvalidOperationException)
                                {
                                    // Socket was closed between receiving a payload and handling it
                                    log.LogVerbose("Received InvalidOperationException from OnPayloadReceived, " +
                                        "stopping receive loop...");

                                    break;
                                }
                                catch (DiscordWebSocketException dwex)
                                {
                                    if (dwex.Error == DiscordWebSocketError.ConnectionClosed)
                                        // Socket was closed while a payload handler was sending another payload
                                        break;
                                    else
                                    {
                                        // Unexpected error occured, we should only let it bubble up if the socket
                                        // is not open after the exception.
                                        if (socket.State == WebSocketState.Open)
                                            log.LogError("[ReceiveLoop] Unexpected error from OnPayloadReceived: " +
                                                $"code = {dwex.Error}, error = {dwex}");
                                        else
                                            // Don't log since that will be handled below
                                            throw;
                                    }
                                }
                            }
                            else
                                log.LogError($"[ReceiveLoop] Failed to parse JSON: \"{message}\"");
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.LogError($"[ReceiveLoop] Uncaught exception: {ex}");
                    OnClosedPrematurely();
                }
            }
        }

        /// <summary>
        /// Parses the message received by the WebSocket.
        /// </summary>
        /// <param name="messageType">The type of message received.</param>
        /// <param name="ms">The stream containing the actual message.</param>
        /// <exception cref="ArgumentException">Thrown if message type is 'Close' or an unknown type.</exception>
        /// <exception cref="IOException">Thrown if the binary message could not be decompressed.</exception>
        async Task<string> ParseMessage(WebSocketMessageType messageType, MemoryStream ms)
        {
            string StreamToString(MemoryStream decompressedMemoryStream)
            {
                ArraySegment<byte> buffer;
                if (!decompressedMemoryStream.TryGetBuffer(out buffer))
                    // The memory stream should be 'exposable' but as a fallback just write the stream to an array.
                    buffer = new ArraySegment<byte>(decompressedMemoryStream.ToArray());

                return Encoding.UTF8.GetString(buffer.Array, buffer.Offset, buffer.Count);
            }

            if (messageType == WebSocketMessageType.Text)
            {
                // Message is already decompressed.
                return StreamToString(ms);
            }
            else if (messageType == WebSocketMessageType.Binary)
            {
                using (MemoryStream decompressed = new MemoryStream())
                {
                    try
                    {
                        // Skip first two bytes
                        ms.Seek(2, SeekOrigin.Begin);

                        // Decompress message
                        using (DeflateStream deflateStream = new DeflateStream(ms, CompressionMode.Decompress, true))
                            await deflateStream.CopyToAsync(decompressed).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new IOException("Failed to decompress binary message.", ex);
                    }

                    // Message is now compressed, return as string.
                    return StreamToString(decompressed);
                }
            }
            else if (messageType == WebSocketMessageType.Close)
                throw new ArgumentException("Message type cannot be \"Close\"", nameof(messageType));
            else
                throw new ArgumentException($"Unknown message type: \"{messageType}\"", nameof(messageType));
        }

        public virtual void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                abortCancellationSource?.Dispose();
                socket.Dispose();
            }
        }
    }
}
