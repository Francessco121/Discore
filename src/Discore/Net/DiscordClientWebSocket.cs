using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Net
{
    class DiscordClientWebSocket : IDisposable
    {
        public WebSocketState State { get { return socket == null ? WebSocketState.None : socket.State; } }

        public event EventHandler<DiscordApiData> OnMessageReceived;
        public event EventHandler OnOpened;
        public event EventHandler OnClosed;
        public event EventHandler<Exception> OnFatalError;

        const int SEND_BUFFER_SIZE = 4 * 1024; // 4kb
        const int RECEIVE_BUFFER_SIZE = 12 * 1024; // 12kb

        DiscordClient client;
        ClientWebSocket socket;

        Thread sendThread;
        Thread receiveThread;

        CancellationTokenSource cancelTokenSource;
        ConcurrentQueue<string> sendQueue;

        DiscordLogger log;

        MemoryStream receiveMs;
        ArraySegment<byte> receiveBuffer;
        byte[] sendBuffer;

        bool isDisposed;

        public DiscordClientWebSocket(DiscordClient client)
        {
            this.client = client;

            log = new DiscordLogger("Client Web Socket");
            sendQueue = new ConcurrentQueue<string>();

            receiveBuffer = new ArraySegment<byte>(new byte[RECEIVE_BUFFER_SIZE]);
            receiveMs = new MemoryStream();
            sendBuffer = new byte[SEND_BUFFER_SIZE];
        }

        public async Task<bool> Connect(string uri)
        {
            log.LogVerbose($"Connecting to {uri}...");

            cancelTokenSource = new CancellationTokenSource();

            sendThread = new Thread(SendLoop);
            sendThread.Name = "DiscordClientWebSocket Send Thread";
            sendThread.IsBackground = true;

            receiveThread = new Thread(ReceiveLoop);
            receiveThread.Name = "DiscordClientWebSocket Receive Thread";
            receiveThread.IsBackground = true;

            socket = new ClientWebSocket();
            socket.Options.Proxy = null;
            socket.Options.KeepAliveInterval = TimeSpan.Zero;

            await socket.ConnectAsync(new Uri(uri), cancelTokenSource.Token);

            if (socket.State == WebSocketState.Open)
            {
                sendThread.Start();
                receiveThread.Start();
                OnOpened?.Invoke(this, EventArgs.Empty);

                return true;
            }

            return false;
        }

        public async Task Close(WebSocketCloseStatus statusCode, string reason)
        {
            if (socket != null && socket.State == WebSocketState.Open)
                await socket.CloseAsync(statusCode, reason, cancelTokenSource.Token);
        }

        public void Send(string json)
        {
            if (json == null)
                throw new ArgumentNullException("json");

            sendQueue.Enqueue(json);
        }

        void ReceiveLoop()
        {
            try
            {
                while (State == WebSocketState.Open && !cancelTokenSource.IsCancellationRequested)
                {
                    WebSocketReceiveResult result = null;

                    // Read full message
                    do
                    {
                        result = socket.ReceiveAsync(receiveBuffer, cancelTokenSource.Token).Result;

                        if (result.MessageType == WebSocketMessageType.Close)
                            throw new DiscoreSocketException(result.CloseStatusDescription, result.CloseStatus.Value);
                        else
                            receiveMs.Write(receiveBuffer.Array, 0, result.Count);
                    }
                    while (result == null || !result.EndOfMessage);

                    // Parse message
                    byte[] array = receiveMs.ToArray();
                    string json = null;

                    if (result.MessageType == WebSocketMessageType.Text)
                        json = Encoding.UTF8.GetString(array, 0, array.Length);
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        // Decompress binary
                        using (MemoryStream compressed = new MemoryStream(array, 2, array.Length - 2))
                        using (MemoryStream decompressed = new MemoryStream())
                        {
                            using (DeflateStream zlib = new DeflateStream(compressed, CompressionMode.Decompress))
                                zlib.CopyTo(decompressed);

                            decompressed.Position = 0;

                            using (StreamReader reader = new StreamReader(decompressed))
                                json = reader.ReadToEnd();
                        }
                    }

                    // Invoke event
                    DiscordApiData data = DiscordApiData.FromJson(json);
                    InvokeOnMessageReceived(data);

                    // Reset memory stream
                    receiveMs.Position = 0;
                    receiveMs.SetLength(0);
                }
            }
            catch (WebSocketException ex)
            {
                LogWebSocketError(ex);
            }
            catch (DiscoreSocketException ex)
            {
                client.EnqueueError(ex);
                OnFatalError?.Invoke(this, ex);
            }
            catch (AggregateException aex)
            {
                WebSocketException wse = aex.InnerException as WebSocketException;
                if (wse != null)
                {
                    // InvalidState from an aggregate exception can only come from the socket receive async,
                    // this is thrown when the socket is canceled. (once the proper .net core websocket implementation
                    // comes out, this can probably be cleaned up).

                    // TODO: When we move to stable release of dotnet core with linux websocket support,
                    // clean this up.
                    if (wse.WebSocketErrorCode != WebSocketError.InvalidState)
                    {
                        LogWebSocketError(wse);
                    }
                }   
            }
            catch (Exception ex)
            {
                client.EnqueueError(ex);
            }
            finally
            {
                Dispose();
            }
        }

        void LogWebSocketError(WebSocketException ex)
        {
            if (IsWebSocketErrorCodeFatal(ex.WebSocketErrorCode))
            {
                log.LogError($"[{GetType().Name}:{ex.WebSocketErrorCode} ({(int)ex.WebSocketErrorCode})] "
                    + "Socket encountered a fatal error, shutting down...");

                socket.Abort();
                OnFatalError?.Invoke(this, ex);
                client.EnqueueError(ex);
            }
        }

        void SendLoop()
        {
            try
            {
                while (State == WebSocketState.Open && !cancelTokenSource.IsCancellationRequested)
                {
                    if (sendQueue.Count > 0)
                    {
                        string json;
                        if (sendQueue.TryDequeue(out json))
                        {
                            int byteCount = Encoding.UTF8.GetBytes(json, 0, json.Length, sendBuffer, 0);
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

                                socket.SendAsync(new ArraySegment<byte>(sendBuffer, offset, count),
                                    WebSocketMessageType.Text, isLast, cancelTokenSource.Token).Wait();
                            }
                        }
                    }
                    else
                        Thread.Sleep(100);
                }
            }
            catch (WebSocketException ex)
            {
                LogWebSocketError(ex);
            }
            catch (AggregateException aex)
            {
                WebSocketException wse = aex.InnerException as WebSocketException;
                if (wse != null)
                {
                    // InvalidState from an aggregate exception can only come from the socket send async,
                    // this is thrown when the socket is canceled. (once the proper .net core websocket implementation
                    // comes out, this can probably be cleaned up).

                    // TODO: When we move to stable release of dotnet core with linux websocket support,
                    // clean this up.
                    if (wse.WebSocketErrorCode != WebSocketError.InvalidState)
                    {
                        LogWebSocketError(wse);
                    }
                }
            }
            catch (Exception ex)
            {
                client.EnqueueError(ex);
            }
            finally
            {
                Dispose();
            }
        }

        bool IsWebSocketErrorCodeFatal(WebSocketError error)
        {
            return error == WebSocketError.ConnectionClosedPrematurely
                || error == WebSocketError.Faulted
                || error == WebSocketError.NativeError;
        }

        void InvokeOnMessageReceived(DiscordApiData data)
        {
            try
            {
                OnMessageReceived?.Invoke(this, data);
            }
            catch (Exception e)
            {
                client.EnqueueError(e);
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;
                cancelTokenSource.Cancel();
                socket.Dispose();

                OnClosed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
