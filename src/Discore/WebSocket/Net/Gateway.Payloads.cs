using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    partial class Gateway
    {
        delegate void PayloadSynchronousCallback(DiscordApiData payload, DiscordApiData data);
        delegate Task PayloadAsynchronousCallback(DiscordApiData payload, DiscordApiData data);

        class PayloadCallback
        {
            public PayloadSynchronousCallback Synchronous { get; }
            public PayloadAsynchronousCallback Asynchronous { get; }

            public PayloadCallback(PayloadSynchronousCallback synchronous)
            {
                Synchronous = synchronous;
            }

            public PayloadCallback(PayloadAsynchronousCallback asynchronous)
            {
                Asynchronous = asynchronous;
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        class PayloadAttribute : Attribute
        {
            public GatewayOPCode OPCode { get; }

            public PayloadAttribute(GatewayOPCode opCode)
            {
                OPCode = opCode;
            }
        }

        Dictionary<GatewayOPCode, PayloadCallback> payloadHandlers;

        void InitializePayloadHandlers()
        {
            payloadHandlers = new Dictionary<GatewayOPCode, PayloadCallback>();

            Type taskType = typeof(Task);
            Type payloadSynchronousType = typeof(PayloadSynchronousCallback);
            Type payloadAsynchronousType = typeof(PayloadAsynchronousCallback);

            foreach (Tuple<MethodInfo, PayloadAttribute> tuple in GetMethodsWithAttribute<PayloadAttribute>())
            {
                MethodInfo method = tuple.Item1;
                PayloadAttribute attr = tuple.Item2;

                PayloadCallback payloadCallback;
                if (method.ReturnType == taskType)
                {
                    Delegate callback = method.CreateDelegate(payloadAsynchronousType, this);
                    payloadCallback = new PayloadCallback((PayloadAsynchronousCallback)callback);
                }
                else
                {
                    Delegate callback = method.CreateDelegate(payloadSynchronousType, this);
                    payloadCallback = new PayloadCallback((PayloadSynchronousCallback)callback);
                }

                payloadHandlers[attr.OPCode] = payloadCallback;
            }
        }

        [Obsolete]
        public void UpdateStatus(string game = null, int? idleSince = null)
        {
            UpdateStatusAsync(game, idleSince).Wait();
        }

        public Task UpdateStatusAsync(string game = null, int? idleSince = null)
        {
            return SendStatusUpdate(game, idleSince);
        }

        [Obsolete]
        public void RequestGuildMembers(Action<IReadOnlyList<DiscordGuildMember>> callback, Snowflake guildId,
            string query = "", int limit = 0)
        {
            RequestGuildMembersAsync(callback, guildId, query, limit).Wait();
        }

        public Task RequestGuildMembersAsync(Action<IReadOnlyList<DiscordGuildMember>> callback, Snowflake guildId,
            string query = "", int limit = 0)
        {
            // Create GUILD_MEMBERS_CHUNK event handler
            EventHandler<DiscordGuildMember[]> eventHandler = null;
            eventHandler = (sender, members) =>
            {
                // Unhook event handler
                OnGuildMembersChunk -= eventHandler;

                // Return members
                callback(members);
            };

            // Hook in event handler
            OnGuildMembersChunk += eventHandler;

            // Send gateway request
            return SendRequestGuildMembersPayload(guildId, query, limit);
        }

        [Payload(GatewayOPCode.Dispatch)]
        async Task HandleDispatchPayload(DiscordApiData payload, DiscordApiData data)
        {
            sequence = payload.GetInteger("s") ?? sequence;
            string eventName = payload.GetString("t");

            DispatchCallback callback;
            if (dispatchHandlers.TryGetValue(eventName, out callback))
            {
                try
                {
                    if (callback.Synchronous != null)
                        callback.Synchronous(data);
                    else
                        await callback.Asynchronous(data).ConfigureAwait(false);
                }
                catch (DiscoreCacheException cex)
                {
                    log.LogWarning($"[{eventName}] Did not complete because: {cex.Message}.");
                }
                catch (Exception ex)
                {
                    log.LogError($"[{eventName}] Unhandled exception: {ex}");
                }
            }
            else
                log.LogWarning($"Missing handler for dispatch event: {eventName}");
        }

        [Payload(GatewayOPCode.Hello)]
        void HandleHelloPayload(DiscordApiData payload, DiscordApiData data)
        {
            heartbeatInterval = data.GetInteger("heartbeat_interval") ?? heartbeatInterval;
            log.LogVerbose($"[Hello] heartbeat_interval: {heartbeatInterval}ms");

            LogServerTrace("Hello", data);

            helloPayloadEvent.Set();
        }

        [Payload(GatewayOPCode.HeartbeatAck)]
        void HandleHeartbeatAckPayload(DiscordApiData payload, DiscordApiData data)
        {
            // Reset heartbeat timeout
            heartbeatTimeoutAt = Environment.TickCount + (heartbeatInterval * HEARTBEAT_TIMEOUT_MISSED_PACKETS);
        }

        [Payload(GatewayOPCode.Reconnect)]
        void HandleReconnectPayload(DiscordApiData payload, DiscordApiData data)
        {
            log.LogInfo("[Reconnect] Performing resume...");

            // We won't worry about sending the resume payload here,
            // as that will be handled by the reconnect procedure,
            // when we pass true.
            BeginResume();
        }

        [Payload(GatewayOPCode.InvalidSession)]
        void HandleInvalidSessionPayload(DiscordApiData payload, DiscordApiData data)
        {
            log.LogInfo("[InvalidSession] Reconnecting...");

            BeginNewSession();
        }

        async Task SendPayload(GatewayOPCode op, DiscordApiData data)
        {
            DiscordApiData payload = new DiscordApiData(DiscordApiDataType.Container);
            payload.Set("op", (int)op);
            payload.Set("d", data);

            await outboundEventRateLimiter.Invoke().ConfigureAwait(false); // Check with the outbound event rate limiter
            socket.Send(payload); // Send payload
        }

        Task SendIdentifyPayload()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("token", app.Authenticator.GetToken());
            data.Set("compress", true);
            data.Set("large_threshold", 250);

            if (app.ShardManager.TotalShardCount > 1)
            {
                DiscordApiData shardData = new DiscordApiData(DiscordApiDataType.Array);
                shardData.Values.Add(new DiscordApiData(shard.Id));
                shardData.Values.Add(new DiscordApiData(app.ShardManager.TotalShardCount));
                data.Set("shard", shardData);
            }

            DiscordApiData props = data.Set("properties", new DiscordApiData(DiscordApiDataType.Container));
            props.Set("$os", RuntimeInformation.OSDescription);
            props.Set("$browser", "discore");
            props.Set("$device", "discore");
            props.Set("$referrer", "");
            props.Set("$referring_domain", "");

            log.LogVerbose("[Identify] Sending payload...");

            return SendPayload(GatewayOPCode.Identify, data);
        }

        Task SendHeartbeatPayload()
        {
            return SendPayload(GatewayOPCode.Heartbeat, new DiscordApiData(sequence));
        }

        Task SendResumePayload()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("token", app.Authenticator.GetToken());
            data.Set("session_id", sessionId);
            data.Set("seq", sequence);

            log.LogVerbose("[Resume] Sending payload...");

            return SendPayload(GatewayOPCode.Resume, data);
        }

        async Task SendStatusUpdate(string game = null, int ? idleSince = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("idle_since", idleSince);

            if (game != null)
            {
                DiscordApiData gameData = new DiscordApiData(DiscordApiDataType.Container);
                data.Set("game", gameData);

                gameData.Set("name", game);
            }

            await gameStatusUpdateRateLimiter.Invoke().ConfigureAwait(false); // Check with the game status update limiter
            await SendPayload(GatewayOPCode.StatusUpdate, data).ConfigureAwait(false); // Send status update
        }

        Task SendRequestGuildMembersPayload(Snowflake guildId, string query, int limit)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("guild_id", guildId);
            data.Set("query", query);
            data.Set("limit", limit);

            return SendPayload(GatewayOPCode.RequestGuildMembers, data);
        }

        internal Task SendVoiceStateUpdatePayload(Snowflake guildId, Snowflake? channelId, bool isMute, bool isDeaf)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("guild_id", guildId);
            data.Set("channel_id", channelId);
            data.Set("self_mute", isMute);
            data.Set("self_deaf", isDeaf);

            return SendPayload(GatewayOPCode.VoiceStateUpdate, data);
        }
    }
}
