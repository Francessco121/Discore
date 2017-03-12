using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Discore.WebSocket.Net
{
    partial class GatewaySocket
    {
        delegate void PayloadCallback(DiscordApiData payload, DiscordApiData data);

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

            Type gatewayType = typeof(Gateway);
            Type payloadType = typeof(PayloadCallback);

            foreach (MethodInfo method in gatewayType.GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                PayloadAttribute attr = method.GetCustomAttribute<PayloadAttribute>();
                if (attr != null)
                    payloadHandlers[attr.OPCode] = (PayloadCallback)method.CreateDelegate(payloadType, this);
            }
        }

        #region Receiving
        [Payload(GatewayOPCode.Dispatch)]
        void HandleDispatchPayload(DiscordApiData payload, DiscordApiData data)
        {
            int sequence = payload.GetInteger("s").Value;
            string eventName = payload.GetString("t");

            OnDispatch?.Invoke(this, new DispatchEventArgs(sequence, eventName, data));
        }

        [Payload(GatewayOPCode.Hello)]
        void HandleHelloPayload(DiscordApiData payload, DiscordApiData data)
        {
            // Set heartbeat interval
            heartbeatInterval = data.GetInteger("heartbeat_interval").Value;
            log.LogVerbose($"[Hello] heartbeat_interval: {heartbeatInterval}ms");
        }

        [Payload(GatewayOPCode.HeartbeatAck)]
        void HandleheartbeatAckPayload(DiscordApiData payload, DiscordApiData data)
        {
            // Reset heartbeat timeout
            heartbeatTimeoutAt = Environment.TickCount + heartbeatInterval;
        }

        [Payload(GatewayOPCode.Reconnect)]
        void HandleReconnectPayload(DiscordApiData payload, DiscordApiData data)
        {
            // Resume
            log.LogInfo("[Reconnect] Performing resume...");
            OnReconnectionRequired?.Invoke(this, false);
        }

        [Payload(GatewayOPCode.InvalidSession)]
        void HandleInvalidSessionPayload(DiscordApiData payload, DiscordApiData data)
        {
            // Start new session
            log.LogInfo("[InvalidSession] Reconnecting...");
            OnReconnectionRequired?.Invoke(this, true);
        }
        #endregion

        #region Sending
        async Task SendPayload(GatewayOPCode op, DiscordApiData data)
        {
            DiscordApiData payload = new DiscordApiData(DiscordApiDataType.Container);
            payload.Set("op", (int)op);
            payload.Set("d", data);

            // Check with the payload rate limiter
            await outboundPayloadRateLimiter.Invoke().ConfigureAwait(false);
            // Send payload
            await SendAsync(payload).ConfigureAwait(false);
        }

        public Task SendIdentifyPayload(string token, int largeThreshold, int shardId, int totalShards)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("token", token);
            data.Set("compress", true);
            data.Set("large_threshold", largeThreshold);

            if (totalShards > 1)
            {
                DiscordApiData shardData = new DiscordApiData(DiscordApiDataType.Array);
                shardData.Values.Add(new DiscordApiData(shardId));
                shardData.Values.Add(new DiscordApiData(totalShards));
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

        public Task SendHeartbeatPayload(int sequence)
        {
            return SendPayload(GatewayOPCode.Heartbeat, new DiscordApiData(sequence));
        }

        public Task SendResumePayload(string token, string sessionId, int sequence)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("token", token);
            data.Set("session_id", sessionId);
            data.Set("seq", sequence);

            log.LogVerbose("[Resume] Sending payload...");

            return SendPayload(GatewayOPCode.Resume, data);
        }

        public async Task SendStatusUpdate(string game = null, int? idleSince = null)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("idle_since", idleSince);

            if (game != null)
            {
                DiscordApiData gameData = new DiscordApiData(DiscordApiDataType.Container);
                data.Set("game", gameData);

                gameData.Set("name", game);
            }

            // Check with the game status update limiter
            await gameStatusUpdateRateLimiter.Invoke().ConfigureAwait(false);
            // Send status update
            await SendPayload(GatewayOPCode.StatusUpdate, data).ConfigureAwait(false);
        }

        public Task SendRequestGuildMembersPayload(Snowflake guildId, string query, int limit)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("guild_id", guildId);
            data.Set("query", query);
            data.Set("limit", limit);

            return SendPayload(GatewayOPCode.RequestGuildMembers, data);
        }

        public Task SendVoiceStateUpdatePayload(Snowflake guildId, Snowflake? channelId, bool isMute, bool isDeaf)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("guild_id", guildId);
            data.Set("channel_id", channelId);
            data.Set("self_mute", isMute);
            data.Set("self_deaf", isDeaf);

            return SendPayload(GatewayOPCode.VoiceStateUpdate, data);
        }
        #endregion
    }
}
