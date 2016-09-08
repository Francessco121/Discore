using Discore.Audio;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Net
{
    delegate void UnhandledGatewayEventHandler(string eventName, DiscordApiData data);

    class GatewaySocket : IDiscordGateway, IDisposable
    {
        public event UnhandledGatewayEventHandler OnUnhandledEvent;
        public event EventHandler<DiscordApiData> OnReadyEvent;
        public event EventHandler<VoiceClientEventArgs> OnVoiceClientConnected;
        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;
        public event EventHandler<Exception> OnFatalError;

        public bool IsConnected { get { return isConnected; } }

        DiscordClient client;
        CancellationTokenSource cancelTokenSource;
        DiscordClientWebSocket socket;
        int heartbeatInterval;
        int sequenceNumber;
        string sessionId;
        int protocolVersion;
        string token;
        DiscordLogger log;

        Thread heartbeatThread;

        int connectionTimeOutAfterMs;
        int lastHeartbeatAckMs;

        bool manuallyDisconnected;
        bool isConnected;

        public GatewaySocket(DiscordClient client)
        {
            this.client = client;

            log = new DiscordLogger("Gateway");

            CreateHeartbeatThread();

            socket = new DiscordClientWebSocket(client, "Gateway");
            cancelTokenSource = new CancellationTokenSource();

            socket.OnMessageReceived += Socket_OnMessageReceived;
            socket.OnOpened += Socket_OnOpened;
            socket.OnFatalError += Socket_OnFatalError;

            isConnected = true;
        }

        void CreateHeartbeatThread()
        {
            heartbeatThread = new Thread(HeartbeatLoop);
            heartbeatThread.Name = "GatewaySocket Heartbeat Thread";
            heartbeatThread.IsBackground = true;
        }

        private void Socket_OnFatalError(object sender, Exception ex)
        {
            DiscoreSocketException socketEx = ex as DiscoreSocketException;
            if (socketEx != null)
            {
                // TODO: Is this necessary?
                DiscordGatewayException gex = new DiscordGatewayException(
                    (GatewayDisconnectCode)socketEx.ErrorCode, socketEx.Message, ex);

                log.LogError(gex);
            }

            HandleFatalError(ex);
        }

        private void Socket_OnOpened(object sender, EventArgs e)
        {
            SendIdentifyPayload(token);

            // The only case where the thread would still be alive is
            // when the socket is reconnecting from timing out.
            if (!heartbeatThread.IsAlive)
                heartbeatThread.Start();
        }

        public async Task<bool> Connect(string token)
        {
            if (socket.State != WebSocketState.Open)
            {
                this.token = token;
                if (await Connect())
                {
                    OnConnected?.Invoke(this, EventArgs.Empty);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Attempts to connect using the stored token.
        /// </summary>
        async Task<bool> Connect()
        {
            if (token == null)
                throw new InvalidOperationException("Cannot connect without a token");

            heartbeatInterval = 0;
            sequenceNumber = 0;

            string gatewayEndpoint = GetGatewayEndpoint();
            if (await socket.Connect($"{gatewayEndpoint}/?encoding=json&v=5"))
                return true;
            else
            {
                TryUpdateGatewayEndpoint(gatewayEndpoint);
                return false;
            }
        }

        void TryUpdateGatewayEndpoint(string currentEndpoint)
        {
            string newEndpoint = client.RestClient.Internal.GetGatewayEndpoint().Result;
            if (newEndpoint != currentEndpoint)
            {
                client.ApiInfo.GatewayEndpoint = newEndpoint;
                client.ApiInfo.Save();
            }
        }

        string GetGatewayEndpoint()
        {
            string endPoint = client.ApiInfo.GatewayEndpoint;
            if (string.IsNullOrWhiteSpace(endPoint))
            {
                endPoint = client.RestClient.Internal.GetGatewayEndpoint().Result;
                client.ApiInfo.GatewayEndpoint = endPoint;
                client.ApiInfo.Save();
            }

            return endPoint;
        }

        public async Task Disconnect()
        {
            if (isConnected)
            {
                log.LogVerbose("Disconnecting from gateway socket...");

                manuallyDisconnected = true;
                isConnected = false;

                cancelTokenSource.Cancel();
                await socket.Close(WebSocketCloseStatus.NormalClosure, "Disconnecting...");

                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        public DiscordVoiceClient ConnectToVoice(DiscordGuildChannel channel)
        {
            DiscordVoiceClient voiceClient = new DiscordVoiceClient(client, channel.Guild);
            client.VoiceClients[channel.Guild] = voiceClient;

            SendVoiceStateUpdate(channel.Guild, channel);

            return voiceClient;
        }

        public void DisconnectFromVoice(DiscordGuild guild)
        {
            SendVoiceStateUpdate(guild, null);
        }

        private void Socket_OnMessageReceived(object sender, DiscordApiData data)
        {
            GatewayOPCode op = (GatewayOPCode)data.GetInteger("op");
            DiscordApiData d = data.Get("d");

            switch (op)
            {
                case GatewayOPCode.Dispath:
                    HandleDispatchPayload(data);
                    break;
                case GatewayOPCode.Hello:
                    HandleHelloPayload(d);
                    break;
                case GatewayOPCode.Reconnect:
                    // Attempt to resume if the server says we lost connection.
                    SendResumePayload();
                    break;
                case GatewayOPCode.InvalidSession:
                    // If we have an invalid session we need to do a full reconnect.
                    SendIdentifyPayload(token);
                    break;
                case GatewayOPCode.Heartbeat:
                case GatewayOPCode.HeartbeatACK:
                    lastHeartbeatAckMs = Environment.TickCount;
                    log.LogHeartbeat("Got heartbeat ack");
                    break;
                default:
                    log.LogWarning($"[Unhanled Op] {op}");
                    break;
            }
        }

        #region SendPayload*
        void SendPayload(GatewayOPCode code, DiscordApiData data)
        {
            DiscordApiData payload = new DiscordApiData();
            payload.Set("op", (int)code);
            payload.Set("d", data);

            socket.Send(payload.SerializeToJson());
        }

        public void SendVoiceStateUpdate(DiscordGuild guild, DiscordGuildChannel voiceChannel)
        {
            DiscordGuildMember member;
            guild.TryGetMember(client.User.Id, out member);

            DiscordVoiceState stateUpdate = new DiscordVoiceState(client, member);
            stateUpdate.Guild = guild;
            stateUpdate.Channel = voiceChannel;
            stateUpdate.User = client.User;
            stateUpdate.SessionId = sessionId;

            SendPayload(GatewayOPCode.VoiceStateUpdate, stateUpdate.Serialize());
        }

        public void SendStatusUpdate(string game, DiscordGameType gameType = DiscordGameType.Default, int? idleSince = null)
        {
            DiscordApiData statusUpdate = new DiscordApiData();
            statusUpdate.Set("idle_since", idleSince);

            if (game != null)
            {
                DiscordApiData gameData = statusUpdate.Set("game", DiscordApiData.CreateContainer());
                gameData.Set("name", game);
                gameData.Set("url", "");
                gameData.Set("type", (int)gameType);
            }

            log.LogVerbose($"[StatusUpdate] Sending status update. Game: '{game}'");
            SendPayload(GatewayOPCode.StatusUpdate, statusUpdate);
        }

        public void RequestOfflineGuildMembers(string guildId, string usernameQuery = "", int limit = 0)
        {
            DiscordApiData requestGuildMembers = new DiscordApiData();
            requestGuildMembers.Set("guild_id", guildId);
            requestGuildMembers.Set("query", usernameQuery);
            requestGuildMembers.Set("limit", limit);

            SendPayload(GatewayOPCode.RequestGuildMembers, requestGuildMembers);
        }

        void SendIdentifyPayload(string token)
        {
            DiscordApiData identify = new DiscordApiData();

            identify.Set("token", token);
            identify.Set("compress", true);
            identify.Set("large_threshold", 250);

            DiscordApiData properties = identify.Set("properties", DiscordApiData.CreateContainer());
            properties.Set("$os", "linux");
            properties.Set("$browser", "discordio_sharp");
            properties.Set("$device", "discordio_sharp");
            properties.Set("$referrer", "");
            properties.Set("$referring_domain", "");

            log.LogVerbose("[IDENTIFY] Sending identify...");
            SendPayload(GatewayOPCode.Identify, identify);
        }

        void SendHeartbeat()
        {
            log.LogHeartbeat($"[HEARTBEAT] Sequence Number: {sequenceNumber}");
            SendPayload(GatewayOPCode.Heartbeat, new DiscordApiData(sequenceNumber));
        }

        void SendResumePayload()
        {
            DiscordApiData resume = new DiscordApiData();
            resume.Set("token", token);
            resume.Set("session_id", sessionId);
            resume.Set("seq", sequenceNumber);

            log.LogImportant("[RESUME] Attempting resume...");
            SendPayload(GatewayOPCode.Resume, resume);
        }
        #endregion

        #region HandlePayload*
        void HandleHelloPayload(DiscordApiData data)
        {
            int heartbeatInterval = data.GetInteger("heartbeat_interval") ?? 0;

            // If we miss 5 heartbeat acknowledgments, it's safe to assume we timed out.
            connectionTimeOutAfterMs = heartbeatInterval * 5;
            lastHeartbeatAckMs = Environment.TickCount;

            this.heartbeatInterval = heartbeatInterval;
            log.LogVerbose($"[HELLO] Heartbeat interval: {heartbeatInterval}ms");
        }

        void HandleDispatchPayload(DiscordApiData data)
        {
            int seq = data.GetInteger("s") ?? 0;
            string eventName = data.GetString("t");
            DiscordApiData d = data.Get("d");

            sequenceNumber = seq;

            if (eventName == "READY")
                HandleReadyEvent(d);
            else if (eventName == "RESUMED")
                HandleResumedEvent(d);
            else if (eventName == "VOICE_SERVER_UPDATE")
                HandleVoiceServerUpdate(d);
            else
                OnUnhandledEvent?.Invoke(eventName, d);
        }

        void HandleReadyEvent(DiscordApiData data)
        {
            protocolVersion = data.GetInteger("v") ?? 0;
            sessionId = data.GetString("session_id");

            log.LogInfo($"[READY] Using API version {protocolVersion} with session {sessionId}.");

            OnReadyEvent?.Invoke(this, data);
        }

        void HandleResumedEvent(DiscordApiData data)
        {
            log.LogImportant("[RESUME] Successfully resumed session.");
        }

        async void HandleVoiceServerUpdate(DiscordApiData data)
        {
            try
            {
                string voiceToken = data.GetString("token");
                string guildId = data.GetString("guild_id");
                string endpoint = data.GetString("endpoint");

                DiscordGuild guild;
                if (client.Cache.TryGet(guildId, out guild))
                {
                    DiscordGuildMember member;
                    if (client.Cache.TryGet(guild, client.User.Id, out member))
                    {
                        DiscordVoiceClient voiceClient;
                        if (client.VoiceClients.TryGetValue(guild, out voiceClient))
                        {
                            if (!voiceClient.IsValid)
                                return;

                            if (!voiceClient.IsConnected)
                            {
                                VoiceSocket voiceSocket = new VoiceSocket(client, member);
                                if (!await voiceSocket.Connect(endpoint, voiceToken))
                                    log.LogError($"Failed to connect voice socket to {endpoint}!");
                                else
                                {
                                    voiceClient.SetSocket(voiceSocket);
                                    OnVoiceClientConnected?.Invoke(this, new VoiceClientEventArgs(voiceClient));
                                }
                            }
                            else
                                log.LogWarning($"[VOICE_SERVER_UPDATE] Attempt to connect a voice client that was already connected!");
                        }
                        else
                            log.LogError($"[VOICE_SERVER_UPDATE] Failed to locate voice client for guild {guild.Id}");
                    }
                    else
                        log.LogError($"[VOICE_SERVER_UPDATE] Failed to locate this programs user with id {client.User.Id}");
                }
                else
                    log.LogWarning($"[VOICE_SERVER_UPDATE] Failed to find guild with id {guildId}");
            }
            catch (Exception ex)
            {
                log.LogError($"[VOICE_SERVER_UPDATE] {ex}");
            }
        }
        #endregion

        void HeartbeatLoop()
        {
            try
            {
                while (heartbeatInterval == 0 && socket.State == WebSocketState.Open && !cancelTokenSource.IsCancellationRequested)
                    Thread.Sleep(1000);

                do
                {
                    while (socket.State == WebSocketState.Open && !cancelTokenSource.IsCancellationRequested)
                    {
                        if (Environment.TickCount >= lastHeartbeatAckMs + connectionTimeOutAfterMs)
                        {
                            log.LogError($"Failed to receive heartbeat acknowledgement after {connectionTimeOutAfterMs}ms");
                            break;
                        }
                        else
                        {
                            SendHeartbeat();
                            Thread.Sleep(heartbeatInterval);
                        }
                    }

                    // Attempt to reconnect
                    if (!cancelTokenSource.IsCancellationRequested)
                    {
                        // Cancel any async operations
                        cancelTokenSource.Cancel();

                        // Disconnect from the gateway fully
                        socket.Close(WebSocketCloseStatus.ProtocolError, "Heartbeat timeout").Wait();

                        // Reset the cancellation source
                        cancelTokenSource = new CancellationTokenSource();

                        // Continuously attempt to reconnect
                        while (!manuallyDisconnected)
                        {
                            if (Connect().Result)
                            {
                                log.LogImportant("Successfully reconnected after timeout");
                                break;
                            }
                            else
                            {
                                log.LogImportant("Failed to reconnect after timeout, waiting 5 seconds before retrying...");
                                Thread.Sleep(5000);
                            }
                        }
                    }

                } while (!cancelTokenSource.IsCancellationRequested && !manuallyDisconnected);
            }
            catch (Exception ex)
            {
                log.LogError(ex);
                HandleFatalError(ex);
            }
        }

        void HandleFatalError(Exception ex)
        {
            // Cancel any async operations
            cancelTokenSource.Cancel();

            // Disconnect from the gateway fully
            try
            {
                socket.Close(WebSocketCloseStatus.InternalServerError, "An internal error occured").Wait();
            }
            catch (Exception)
            {
                // TODO: may need to recreate the socket here?
            }

            log.LogError("Gateway encountered a fatal error");
            log.LogImportant("Waiting for heartbeat loop to end before reconnecting...");

            try
            {
                // Wait for heartbeat thread to stop
                heartbeatThread.Join();

                // Recreate the heartbeat thread
                CreateHeartbeatThread();

                // Reset the cancellation source
                cancelTokenSource = new CancellationTokenSource();

                // Attempt to reconnect using existing token
                if (Connect().Result)
                    log.LogImportant("Successfully reconnected after fatal error");
                else
                {
                    log.LogError("Failed to reconnect after fatal error");
                    isConnected = false;
                    OnFatalError?.Invoke(this, ex);
                }
            }
            catch (Exception _ex)
            {
                log.LogError($"Failed to reconnect after fatal error: {_ex}");
                isConnected = false;
                OnFatalError?.Invoke(this, new AggregateException(_ex, ex));
            }
        }

        public void Dispose()
        {
            socket.Dispose();
        }
    }
}
