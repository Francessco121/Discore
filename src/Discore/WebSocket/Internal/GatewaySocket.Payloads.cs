using Newtonsoft.Json;
using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

#pragma warning disable IDE0051 // Remove unused private members

namespace Discore.WebSocket.Internal
{
    partial class GatewaySocket
    {
        static readonly Random rnd = new Random();

        #region Receiving
        [Payload(GatewayOPCode.Dispatch)]
        void HandleDispatchPayload(JsonElement payload, JsonElement data)
        {
            sequence = payload.GetProperty("s").GetInt32();
            string eventName = payload.GetProperty("t").GetString()!;

            OnDispatch?.Invoke(this, new DispatchEventArgs(eventName, data));
        }

        [Payload(GatewayOPCode.Hello)]
        async Task HandleHelloPayload(JsonElement payload, JsonElement data)
        {
            if (!receivedHello)
            {
                receivedHello = true;

                // Set heartbeat interval
                heartbeatInterval = data.GetProperty("heartbeat_interval").GetInt32();
                log.LogVerbose($"[Hello] heartbeat_interval = {heartbeatInterval}ms");

                // Begin heartbeat loop
                heartbeatCancellationSource = new CancellationTokenSource();
                heartbeatTask = HeartbeatLoop();

                // Notify so the IDENTIFY or RESUME payloads are sent
                if (OnHello != null)
                    await OnHello.Invoke();
            }
            else
                log.LogWarning("Received more than one HELLO payload.");
        }

        [Payload(GatewayOPCode.Heartbeat)]
        async Task HandleHeartbeatPayload(JsonElement payload, JsonElement data)
        {
            // The gateway can request a heartbeat in certain (unlisted) scenarios.
            log.LogVerbose("[Heartbeat] Gateway requested heartbeat.");

            await SendHeartbeatPayload();
        }

        [Payload(GatewayOPCode.HeartbeatAck)]
        void HandleHeartbeatAckPayload(JsonElement payload, JsonElement data)
        {
            receivedHeartbeatAck = true;
        }

        [Payload(GatewayOPCode.Reconnect)]
        void HandleReconnectPayload(JsonElement payload, JsonElement data)
        {
            // Resume
            log.LogInfo("[Reconnect] Performing resume...");
            OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(false));
        }

        [Payload(GatewayOPCode.InvalidSession)]
        void HandleInvalidSessionPayload(JsonElement payload, JsonElement data)
        {
            bool isResumable = data.GetBoolean();

            if (isResumable)
            {
                // Resume
                log.LogInfo("[InvalidSession] Resuming...");
                OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(false, 5000));
            }
            else
            {
                // Start new session
                log.LogInfo("[InvalidSession] Starting new session...");
                OnReconnectionRequired?.Invoke(this, new ReconnectionEventArgs(true, rnd.Next(1000, 5001)));
            }
        }
        #endregion

        #region Sending
        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        /// <exception cref="JsonWriterException">Thrown if the given data cannot be serialized as JSON.</exception>
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

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        Task SendHeartbeatPayload()
        {
            return SendPayload(GatewayOPCode.Heartbeat, new DiscordApiData(sequence));
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public async Task SendIdentifyPayload(string token, int largeThreshold, int shardId, int totalShards)
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

            log.LogVerbose("[Identify] Sending payload...");

            // Make sure we don't send IDENTIFY's too quickly
            await identifyRateLimiter.Invoke(CancellationToken.None).ConfigureAwait(false);
            // Send IDENTIFY
            await SendPayload(GatewayOPCode.Identify, data).ConfigureAwait(false);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendResumePayload(string token, string sessionId, int sequence)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("token", token);
            data.Set("session_id", sessionId);
            data.Set("seq", sequence);

            log.LogVerbose("[Resume] Sending payload...");

            return SendPayload(GatewayOPCode.Resume, data);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public async Task SendStatusUpdate(StatusOptions options)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("since", options.AfkSince);
            data.Set("afk", options.Afk);
            data.Set("status", options.GetStatusString());

            if (options.Game != null)
            {
                DiscordApiData gameData = new DiscordApiData(DiscordApiDataType.Container);
                data.Set("game", gameData);

                gameData.Set("name", options.Game.Name);
                gameData.Set("type", (int)options.Game.Type);
                gameData.Set("url", options.Game.Url);
            }

            // Check with the game status update limiter
            await gameStatusUpdateRateLimiter.Invoke().ConfigureAwait(false);
            // Send status update
            await SendPayload(GatewayOPCode.StatusUpdate, data).ConfigureAwait(false);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendRequestGuildMembersPayload(Snowflake guildId, string query, int limit)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.SetSnowflake("guild_id", guildId);
            data.Set("query", query);
            data.Set("limit", limit);

            return SendPayload(GatewayOPCode.RequestGuildMembers, data);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendVoiceStateUpdatePayload(Snowflake guildId, Snowflake? channelId, bool isMute, bool isDeaf)
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.SetSnowflake("guild_id", guildId);
            data.SetSnowflake("channel_id", channelId);
            data.Set("self_mute", isMute);
            data.Set("self_deaf", isDeaf);

            return SendPayload(GatewayOPCode.VoiceStateUpdate, data);
        }
        #endregion
    }
}

#pragma warning restore IDE0051 // Remove unused private members

#nullable restore
