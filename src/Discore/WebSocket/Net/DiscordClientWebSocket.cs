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
        const int SEND_BUFFER_SIZE = 4 * 1024;  // 4kb (Discord's max payload size)
        const int RECEIVE_BUFFER_SIZE = 12 * 1024; // 12kb

        ClientWebSocket socket;

        CancellationTokenSource abortCancellationSource;
        Task receiveTask;

        AsyncLock sendLock;

        DiscoreLogger log;

        protected DiscordClientWebSocket(DiscoreLogger log)
        {
            this.log = log;

            sendLock = new AsyncLock();

            socket = new ClientWebSocket();
            // Disable "keep alive" packets, Discord's WebSocket servers do not handle
            // these and will disconnect us with an 'decode error'.
            socket.Options.KeepAliveInterval = TimeSpan.Zero;
        }

        /// <summary>
        /// Called when a payload has been received successfully.
        /// </summary>
        protected abstract void OnPayloadReceived(DiscordApiData payload);
        /// <summary>
        /// Called when a close message has been received. The socket will be closed automatically after this call.
        /// </summary>
        protected abstract void OnCloseReceived(WebSocketCloseStatus closeStatus, string closeDescription);
        /// <summary>
        /// Called when a WebSocket error occurs when receiving a message. The socket will be in the CloseSent state.
        /// </summary>
        protected abstract void OnError(WebSocketError error, string message, int nativeErrorCode);
        /// <summary>
        /// Called when the receive task ends from an unexpected exception. The socket may still be connected.
        /// </summary>
        protected abstract void OnUnexpectedError(Exception ex);

        /// <param name="cancellationToken">Token that when cancelled will abort the entire socket.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="uri"/> does not start with ws:// or wss://.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="uri"/> is null.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the socket attempts to start after a first time. A WebSocket instance
        /// can only be used for one connection attempt.
        /// </exception>
        /// <exception cref="TaskCanceledException"></exception>
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
                log.LogError($"Socket.ConnectAsync succeeded but the state is {socket.State}!");
                throw new WebSocketException(WebSocketError.Faulted, "Failed to connect. No other information is available.");
            }

            // Connection successful (exception would be thrown otherwise)
            abortCancellationSource = new CancellationTokenSource();

            // Start receive task
            receiveTask = ReceiveLoop();

            log.LogVerbose("Successfully connected.");
        }

        /// <param name="cancellationToken">Token that when cancelled will abort the entire socket.</param>
        /// <exception cref="TaskCanceledException"></exception>
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
                        log.LogWarning($"Socket failed to disconnect after 5s, aborting...");
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
                        if (ex is WebSocketException wsex && wsex.WebSocketErrorCode == WebSocketError.InvalidState)
                            // The socket being aborted is normal.
                            continue;

                        log.LogError($"Uncaught exception found while disconnecting: {ex}");
                    }
                }

                log.LogVerbose("Disconnected.");
            });
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        /// <exception cref="JsonWriterException">Thrown if the given data cannot be serialized as JSON.</exception>
        public async Task SendAsync(DiscordApiData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (socket.State != WebSocketState.Open)
                throw new InvalidOperationException("Cannot send data when the socket is not open.");

            // Serialize the data as JSON and convert to bytes
            byte[] bytes = Encoding.UTF8.GetBytes(data.SerializeToJson());

            // Wait for any existing send operations,
            // ClientWebSocket only supports one send operation at a time.
            using (await sendLock.LockAsync())
            {
                // Now that we have acquired the lock, check if the socket is still open.
                // If not, just ignore this message so we can effectively cancel any pending sends after close.
                if (socket.State == WebSocketState.Open)
                {
                    try
                    {
                        await SendData(bytes);
                    }
                    catch (Exception ex)
                    {
                        // We do not want any other exceptions bubbling up,
                        // the SendData method should handle ALL exceptions.
                        log.LogError($"[SendAsync] Unexpected error while sending: {ex}");
                    }
                }
            }
        }

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

                try
                {
                    // Can only throw:
                    //   OperationCanceledException (if aborted)
                    //   WebSocketException (if stream closed)
                    //   ObjectDisposedException (if disposed)
                    //   InvalidOperationException (if not connected)
                    await socket.SendAsync(arraySeg, WebSocketMessageType.Text,
                        isLast, abortCancellationSource.Token).ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    log.LogVerbose($"[SendData] Socket was disposed while sending.");
                    break;
                }
                catch (OperationCanceledException)
                {
                    log.LogVerbose($"[SendData] Socket was aborted while sending.");
                    break;
                }
                catch (WebSocketException wsex)
                {
                    // Only error here should be 'ConnectionClosedPrematurely',
                    // in this case we should just stop the send loop.

                    if (wsex.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
                        log.LogError($"[SendData] Unexpected error: {wsex}");

                    break;
                }
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

                                OnError(wsex.WebSocketErrorCode, wsex.Message, wsex.ErrorCode);
                                break;
                            }

                            if (result.MessageType == WebSocketMessageType.Close)
                            {
                                // Server is disconnecting us
                                isClosing = true;

                                // Notify inheriting object
                                OnCloseReceived(result.CloseStatus.Value, result.CloseStatusDescription);
                                break;
                            }
                            else
                                // Data message, append buffer to memory stream
                                ms.Write(buffer.Array, 0, result.Count);
                        }
                        while (socket.State == WebSocketState.Open && !result.EndOfMessage);

                        if (isClosing)
                        {
                            try
                            {
                                log.LogVerbose("[ReceiveLoop] Completing close handshake with status NormalClosure (1000)...");

                                // Complete the closing handshake
                                // TODO: Check that a 'Normal closure' here won't ALWAYS force us to create a new gateway session,
                                // it is only documented that this happens when we INITIATE the closing handshake.
                                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", abortCancellationSource.Token);

                                log.LogVerbose("[ReceiveLoop] Completed close handshake.");
                            }
                            catch (Exception ex)
                            {
                                log.LogError($"[ReceiveLoop] Failed to complete closing handshake: {ex}");
                            }

                            break;
                        }
                        else
                        {
                            // Parse the message
                            string message = null;

                            try
                            {
                                message = await ParseMessage(result.MessageType, ms);
                            }
                            catch (Exception ex)
                            {
                                log.LogError($"[ReceiveLoop] Failed to parse message: {ex}");
                            }

                            if (message != null)
                            {
                                if (DiscordApiData.TryParseJson(message, out DiscordApiData data))
                                    // Notify inheriting object that a payload has been received.
                                    OnPayloadReceived(data);
                                else
                                    log.LogError($"[ReceiveLoop] Failed to parse JSON: \"{message}\"");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.LogError($"[ReceiveLoop] Uncaught exception: {ex}");
                    OnUnexpectedError(ex);
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
            abortCancellationSource?.Dispose();
            socket.Dispose();
        }
    }
}
