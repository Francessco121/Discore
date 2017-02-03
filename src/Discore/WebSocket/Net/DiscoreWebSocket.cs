using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    class DiscoreWebSocket : IDisposable
    {
        public DiscoreWebSocketState State { get; private set; }

        public event EventHandler<Uri> OnConnected;
        public event EventHandler<WebSocketCloseStatus> OnDisconnected;
        public event EventHandler<DiscordApiData> OnMessageReceived;
        public event EventHandler<Exception> OnError;

        /// <summary>
        /// When true, the procedure of handling socket errors that eventually
        /// calls OnError will not run.
        /// </summary>
        public bool IgnoreSocketErrors { get; set; } // Disables the HandleFatalException method.

        const int SEND_BUFFER_SIZE      = 4 * 1024;  // 4kb (max gateway payload size)
        const int RECEIVE_BUFFER_SIZE   = 12 * 1024; // 12kb

        ClientWebSocket socket;
        CancellationTokenSource taskCancelTokenSource;

        bool runTasks;
        bool tasksEndingFromError;
        Task sendTask;
        Task receiveTask;

        ConcurrentQueue<DiscordApiData> sendQueue;

        DiscoreLogger log;
        WebSocketDataType dataType;

        bool isDisposed;

        internal DiscoreWebSocket(WebSocketDataType dataType, string loggingName)
        {
            if (dataType != WebSocketDataType.Json)
                throw new NotImplementedException("Only JSON packets are supported so far.");

            this.dataType = dataType;
            log = new DiscoreLogger($"WebSocket:{loggingName}");

            State = DiscoreWebSocketState.Closed;
        }

        /// <summary>
        /// Attempts to connect the WebSocket to the specified url.
        /// Invokes the OnConnected event when successful.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown if uri is not a valid WebSocket uri.</exception>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="url"/> is null.</exception>
        /// <exception cref="UriFormatException">Thrown if <paramref name="url"/> is not a valid uri.</exception>
        /// <exception cref="InvalidOperationException">Thrown if this WebSocket is not closed.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if this WebSocket has been disposed.</exception>
        /// <param name="cancellationToken">Token to cancel the connection attempt.</param>
        public async Task<bool> ConnectAsync(string url, CancellationToken cancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(socket));

            if (url == null)
                throw new ArgumentNullException(nameof(url), "Url cannot be null.");

            if (State != DiscoreWebSocketState.Closed)
                throw new InvalidOperationException("Failed to connect, the WebSocket is already connected or connecting.");

            Uri uri = new Uri(url);

            State = DiscoreWebSocketState.Connecting;
            log.LogVerbose($"[ConnectAsync] Connecting to {url}...");
            
            try
            {
                // Create new socket (old socket's can't be reset)
                socket = new ClientWebSocket();
                socket.Options.Proxy = null;
                socket.Options.KeepAliveInterval = TimeSpan.Zero;

                // Connect
                await socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
            }
            catch (WebSocketException) { /* Connection failed, but this is not critical. */ }
            catch
            {
                State = DiscoreWebSocketState.Closed;
                throw;
            }

            // If connect was successful, start send/receive threads
            if (socket.State == WebSocketState.Open)
            {
                // Create state for tasks
                taskCancelTokenSource = new CancellationTokenSource();
                sendQueue = new ConcurrentQueue<DiscordApiData>();

                // Start new tasks for sending/receiving.
                runTasks = true;
                sendTask = SendLoop();
                receiveTask = ReceiveLoop();

                // Flip state
                State = DiscoreWebSocketState.Open;
                log.LogVerbose($"[ConnectAsync] Connected to {url}.");

                // Fire event and return
                OnConnected?.Invoke(this, uri);
                return true;
            }
            else
            {
                State = DiscoreWebSocketState.Closed;
                log.LogError($"[ConnectAsync] Failed to connect to {url}.");
                return false;
            } 
        }

        /// <exception cref="InvalidOperationException">Thrown if this WebSocket is not open.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if this WebSocket has been disposed.</exception>
        public async Task DisconnectAsync(CancellationToken cancellationToken)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(socket));

            if (State != DiscoreWebSocketState.Open)
                throw new InvalidOperationException("Failed to disconnect, the WebSocket is not open.");

            log.LogVerbose("[DisconnectAsync] Disconnecting...");

            // Close the socket.
            try { await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting...", cancellationToken).ConfigureAwait(false); }
            catch (TaskCanceledException) { throw; }
            catch { }

            // Cancel send/receive tasks
            StopTasks();

            // Do not await tasks if we are coming from an error,
            // this avoids a deadlock if this is called from one of the tasks.
            if (!tasksEndingFromError)
            {
                log.LogVerbose("[DisconnectAsync] Awaiting sendTask and receiveTask...");

                // Wait for each task to end.
                await Task.WhenAll(sendTask, receiveTask).ConfigureAwait(false);
            }

            // Set our state
            State = DiscoreWebSocketState.Closed;

            // Fire event and return
            log.LogVerbose("[DisconnectAsync] Disconnected.");
            OnDisconnected?.Invoke(this, WebSocketCloseStatus.NormalClosure);
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ObjectDisposedException">Thrown if this WebSocket has been disposed.</exception>
        public void Send(DiscordApiData data)
        {
            if (isDisposed)
                throw new ObjectDisposedException(nameof(socket));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            sendQueue.Enqueue(data);
        }

        async Task SendLoop()
        {
            try
            {
                while (runTasks)
                {
                    if (sendQueue.Count > 0)
                    {
                        DiscordApiData data;
                        if (sendQueue.TryDequeue(out data))
                        {
                            byte[] bytes = null;
                            if (dataType == WebSocketDataType.Json)
                                bytes = Encoding.UTF8.GetBytes(data.SerializeToJson());

                            // TODO: ETF serialization

                            // Send the payload
                            int byteCount = bytes.Length;
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

                                WebSocketMessageType msgType = dataType == WebSocketDataType.Json 
                                    ? WebSocketMessageType.Text 
                                    : WebSocketMessageType.Binary;

                                ArraySegment<byte> arraySeg = new ArraySegment<byte>(bytes, offset, count);

                                try
                                {
                                    await socket.SendAsync(arraySeg, msgType, isLast, taskCancelTokenSource.Token).ConfigureAwait(false);
                                }
                                catch (WebSocketException wsex)
                                {
                                    if (wsex.WebSocketErrorCode != WebSocketError.Success           // Not success
                                        && wsex.WebSocketErrorCode != WebSocketError.InvalidState)  // Not cancel/abort
                                    {
                                        await HandleFatalException(wsex, sendTask).ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                    else
                        await Task.Delay(100, taskCancelTokenSource.Token).ConfigureAwait(false);
                }
            }
            catch (TaskCanceledException) { /* Socket is disconnecting */ }
            catch (Exception ex)
            {
                // Handle the exception
                await HandleFatalException(ex, sendTask).ConfigureAwait(false);
            }
        }

        async Task ReceiveLoop()
        {
            try
            {
                ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[RECEIVE_BUFFER_SIZE]);

                using (MemoryStream receiveMs = new MemoryStream())
                {
                    while (runTasks)
                    {
                        WebSocketReceiveResult result = null;

                        // Reset receive stream for next packet
                        receiveMs.Position = 0;
                        receiveMs.SetLength(0);

                        bool isClosing = false;

                        // Receive until a full message is read
                        do
                        {
                            try
                            {
                                result = await socket.ReceiveAsync(receiveBuffer, taskCancelTokenSource.Token).ConfigureAwait(false);
                            }
                            catch (TaskCanceledException)
                            {
                                // Socket is disconnecting, end the loop.
                                isClosing = true;
                                break;
                            }
                            catch (WebSocketException wsex)
                            {
                                if (wsex.WebSocketErrorCode != WebSocketError.Success           // Not success
                                    && wsex.WebSocketErrorCode != WebSocketError.InvalidState)  // Not cancel/abort
                                {
                                    await HandleFatalException(wsex, receiveTask).ConfigureAwait(false);
                                }
                            }

                            if (result != null)
                            {
                                if (result.MessageType == WebSocketMessageType.Close)
                                {
                                    isClosing = true;

                                    // If the close status was not normal, treat it as an error
                                    if (result.CloseStatus != WebSocketCloseStatus.NormalClosure)
                                        throw new DiscoreWebSocketException(result.CloseStatusDescription, 
                                            result.CloseStatus ?? WebSocketCloseStatus.Empty);
                                    else
                                        // If normal, just make sure everything else stops.
                                        StopTasks();
                                }
                                else
                                    receiveMs.Write(receiveBuffer.Array, 0, result.Count);
                            }

                        } while (result == null || !result.EndOfMessage);

                        if (isClosing)
                            break;

                        // Parse message
                        string str;
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            ArraySegment<byte> packet;
                            if (!receiveMs.TryGetBuffer(out packet))
                                // This should never ever ever get called, 
                                // but just incase since .GetBuffer() isn't available in dotnet core.
                                packet = new ArraySegment<byte>(receiveMs.ToArray());

                            str = Encoding.UTF8.GetString(packet.Array, 0, packet.Count);
                        }
                        else
                        {
                            // Decompress binary
                            using (MemoryStream decompressed = new MemoryStream())
                            {
                                // Skip first two bytes
                                receiveMs.Seek(2, SeekOrigin.Begin);

                                // Decompress packet
                                using (DeflateStream deflateStream = new DeflateStream(receiveMs, CompressionMode.Decompress, true))
                                    await deflateStream.CopyToAsync(decompressed, 81920, taskCancelTokenSource.Token).ConfigureAwait(false);

                                decompressed.Position = 0;

                                // Read decompressed packet as string
                                using (StreamReader reader = new StreamReader(decompressed))
                                    str = await reader.ReadToEndAsync().ConfigureAwait(false);
                            }
                        }

                        // Parse string and invoke OnMessageReceived
                        DiscordApiData data = null;
                        if (dataType == WebSocketDataType.Json)
                        {
                            if (!DiscordApiData.TryParseJson(str, out data))
                                log.LogError($"Failed to parse json: {str}");
                        }

                        // TODO: ETF deserialization

                        if (data != null)
                            OnMessageReceived?.Invoke(this, data);
                    }
                }
            }
            catch (TaskCanceledException) { }
            catch (Exception ex)
            {
                // Handle the exception
                await HandleFatalException(ex, receiveTask).ConfigureAwait(false);
            }
        }

        void StopTasks()
        {
            runTasks = false;
            taskCancelTokenSource.Cancel();
        }

        async Task HandleFatalException(Exception ex, Task originatingTask)
        {
            // Ensure we are allowed to process these errors.
            // Don't let two tasks enter the handler at the same time.
            if (!IgnoreSocketErrors && !tasksEndingFromError)
            {
                tasksEndingFromError = true;

                try
                {
                    StopTasks();

                    // Ensure the other task finishes, the 'originating task'
                    // will end after this method completes.
                    if (originatingTask == sendTask)
                    {
                        log.LogVerbose("[HandleFatalException] Awaiting receiveTask...");
                        await receiveTask.ConfigureAwait(false);
                    }
                    else
                    {
                        log.LogVerbose("[HandleFatalException] Awaiting sendTask...");
                        await sendTask.ConfigureAwait(false);
                    }

                    // Log the error
                    log.LogError(ex);

                    // Disconnect socket if still connected
                    if (State == DiscoreWebSocketState.Open)
                        await DisconnectAsync(CancellationToken.None).ConfigureAwait(false);

                    // Fire event
                    OnError?.Invoke(this, ex);
                }
                finally
                {
                    tasksEndingFromError = false;
                }
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                socket?.Dispose();
                taskCancelTokenSource?.Dispose();

                runTasks = false;
                State = DiscoreWebSocketState.Closed;
            }
        }
    }
}
