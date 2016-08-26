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

        public bool IsConnected { get { return socket.State == WebSocketState.Open || socket.State == WebSocketState.Connecting; } }

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

        public GatewaySocket(DiscordClient client)
        {
            this.client = client;

            log = new DiscordLogger("Gateway");

            heartbeatThread = new Thread(HeartbeatLoop);
            heartbeatThread.Name = "GatewaySocket Heartbeat Thread";
            heartbeatThread.IsBackground = true;

            socket = new DiscordClientWebSocket(client);
            cancelTokenSource = new CancellationTokenSource();

            socket.OnMessageReceived += Socket_OnMessageReceived;
            socket.OnOpened += Socket_OnOpened;
        }

        private void Socket_OnOpened(object sender, EventArgs e)
        {
            SendIdentifyPayload(token);
            heartbeatThread.Start();
        }

        public async Task<bool> Connect(string token)
        {
            if (socket.State != WebSocketState.Open)
            {
                this.token = token;
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

            return false;
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
            await socket.Close(1000, "Disconnecting...");
            log.LogVerbose("Disconnecting from gateway socket...");
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
            DiscordVoiceState stateUpdate = new DiscordVoiceState(client);
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
                DiscordApiData gameData = statusUpdate.CreateNestedContainer("game");
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

            DiscordApiData properties = identify.CreateNestedContainer("properties");
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
            heartbeatInterval = data.GetInteger("heartbeat_interval") ?? 0;
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

                while (socket.State == WebSocketState.Open && !cancelTokenSource.IsCancellationRequested)
                {
                    SendHeartbeat();
                    Thread.Sleep(heartbeatInterval);
                }
            }
            catch (Exception e)
            {
                client.EnqueueError(e);
            }
        }

        public void Dispose()
        {
            socket.Dispose();
        }
    }
}
