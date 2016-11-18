using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace Discore.WebSocket.Net
{
    class DiscoreWebSocket : IDisposable
    {
        public WebSocketState State { get { return socket != null ? socket.State : WebSocketState.None; } }

        public event EventHandler<Uri> OnConnected;
        public event EventHandler<WebSocketCloseStatus> OnDisconnected;
        public event EventHandler<DiscordApiData> OnMessageReceived;
        public event EventHandler<Exception> OnError;

        const int SEND_BUFFER_SIZE      = 4 * 1024;  // 4kb
        const int RECEIVE_BUFFER_SIZE   = 12 * 1024; // 12kb

        ClientWebSocket socket;
        CancellationTokenSource cancelTokenSource;

        Thread sendThread;
        Thread receiveThread;

        ConcurrentQueue<DiscordApiData> sendQueue;

        DiscoreLogger log;
        WebSocketDataType dataType;

        bool isDisposed;

        internal DiscoreWebSocket(WebSocketDataType dataType, string loggingName)
        {
            if (dataType != WebSocketDataType.Json)
                throw new NotImplementedException("Only json packets are supported so far.");

            this.dataType = dataType;

            log = new DiscoreLogger($"WebSocket:{loggingName}");

            sendQueue = new ConcurrentQueue<DiscordApiData>();
        }

        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="UriFormatException"></exception>
        public bool Connect(string url)
        {
            if (State == WebSocketState.None || State > WebSocketState.Open)
            {
                // Create new socket (old socket's can't be reset)
                socket = new ClientWebSocket();
                socket.Options.Proxy = null;
                socket.Options.KeepAliveInterval = TimeSpan.Zero;

                log.LogInfo($"Connecting to {url}...");
                Uri uri = new Uri(url);

                // Reset fields
                cancelTokenSource = new CancellationTokenSource();
                ClearQueue();

                // Connect
                try
                {
                    socket.ConnectAsync(uri, cancelTokenSource.Token).Wait();
                }
                catch (AggregateException aex)
                {
                    throw aex.InnerException;
                }

                // If connect was successful, start send/receive threads
                if (socket.State == WebSocketState.Open)
                {
                    sendThread = new Thread(SendLoop);
                    sendThread.Name = "DiscoreWebSocket Send Thread";
                    sendThread.IsBackground = true;

                    receiveThread = new Thread(ReceiveLoop);
                    receiveThread.Name = "DiscoreWebSocket Receive Thread";
                    receiveThread.IsBackground = true;

                    sendThread.Start();
                    receiveThread.Start();

                    log.LogInfo($"Connected to {url}.");

                    OnConnected?.Invoke(this, uri);
                    return true;
                }
                else
                {
                    log.LogInfo($"Failed to connect to {url}.");
                    return false;
                }
            }
            else
            {
                log.LogWarning($"Attempted to connect during invalid socket state: {socket.State}");
                return false;
            }
        }

        public bool Disconnect()
        {
            if (State == WebSocketState.Open)
            {
                cancelTokenSource.Cancel();
                
                try
                {
                    socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting...", CancellationToken.None).Wait();
                    OnDisconnected?.Invoke(this, WebSocketCloseStatus.NormalClosure);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                log.LogWarning($"Attempted to disconnect during invalid socket state: {State}");
                return false;
            }
        }

        /// <exception cref="ArgumentNullException"></exception>
        public void Send(DiscordApiData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            sendQueue.Enqueue(data);
        }

        void ClearQueue()
        {
            DiscordApiData temp;
            while (sendQueue.TryDequeue(out temp)) ;
        }

        void SendLoop()
        {
            try
            {
                while (State == WebSocketState.Open)
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

                                try
                                {
                                    socket.SendAsync(new ArraySegment<byte>(bytes, offset, count),
                                        (dataType == WebSocketDataType.Json 
                                            ? WebSocketMessageType.Text 
                                            : WebSocketMessageType.Binary), 
                                        isLast, cancelTokenSource.Token).Wait();
                                }
                                catch (AggregateException aex)
                                {
                                    throw aex.InnerException;
                                }
                            }
                        }
                    }
                    else
                        Thread.Sleep(100);
                }
            }
            catch (OperationCanceledException) { } // If canceled, assume other thread is shutting down
            catch (WebSocketException wsex)
            {
                if (wsex.WebSocketErrorCode != WebSocketError.Success           // Not success
                    && wsex.WebSocketErrorCode != WebSocketError.InvalidState)  // Not cancel/abort
                {
                    HandleFatalException(wsex);
                }
            }
            catch (Exception ex)
            {
                HandleFatalException(ex);
            }
        }

        void ReceiveLoop()
        {
            try
            {
                ArraySegment<byte> receiveBuffer = new ArraySegment<byte>(new byte[RECEIVE_BUFFER_SIZE]);

                using (MemoryStream receiveMs = new MemoryStream())
                {
                    while (State == WebSocketState.Open)
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
                                result = socket.ReceiveAsync(receiveBuffer, cancelTokenSource.Token).Result;
                            }
                            catch (AggregateException aex)
                            {
                                throw aex.InnerException;
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
                                        cancelTokenSource.Cancel();
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
                                    deflateStream.CopyTo(decompressed);

                                decompressed.Position = 0;

                                // Read decompressed packet as string
                                using (StreamReader reader = new StreamReader(decompressed))
                                    str = reader.ReadToEnd();
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
                        {
                            try
                            {
                                OnMessageReceived?.Invoke(this, data);
                            }
                            catch (Exception ex)
                            {
                                log.LogError($"[OnMessageReceived] Uncaught exception: {ex}");
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException) { } // If canceled, assume other thread is shutting down
            catch (WebSocketException wsex)
            {
                if (wsex.WebSocketErrorCode != WebSocketError.Success           // Not success
                    && wsex.WebSocketErrorCode != WebSocketError.InvalidState)  // Not cancel/abort
                {
                    HandleFatalException(wsex);
                }
            }
            catch (Exception ex)
            {
                HandleFatalException(ex);
            }
        }

        void HandleFatalException(Exception ex)
        {
            // Log the error
            log.LogError(ex);

            // Cancel all operations
            cancelTokenSource.Cancel();

            // Try to close the socket if still open.
            if (State == WebSocketState.Open)
            {
                try
                {
                    socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "An internal error occured",
                        CancellationToken.None).Wait();

                    OnDisconnected?.Invoke(this, WebSocketCloseStatus.InternalServerError);
                }
                catch { }
            }

            OnError?.Invoke(this, ex);
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                socket?.Dispose();

                cancelTokenSource.Cancel();
            }
        }
    }
}
