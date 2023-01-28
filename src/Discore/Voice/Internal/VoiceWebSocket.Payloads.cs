using Discore.WebSocket;
using Nito.AsyncEx;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0060 // Remove unused parameter

namespace Discore.Voice.Internal
{
    partial class VoiceWebSocket
    {
        public AsyncCollection<int> HelloQueue { get; } =
            new AsyncCollection<int>();

        public AsyncCollection<VoiceReadyEventArgs> ReadyQueue { get; } =
            new AsyncCollection<VoiceReadyEventArgs>();

        public AsyncCollection<VoiceSessionDescriptionEventArgs> SessionDescriptionQueue { get; } =
            new AsyncCollection<VoiceSessionDescriptionEventArgs>();

        public AsyncCollection<object?> ResumedQueue { get; } =
            new AsyncCollection<object?>();

		[Payload(VoiceOPCode.Ready)]
        async Task HandleReadyPayload(JsonElement payload, JsonElement data)
        {
            var ip = IPAddress.Parse(data.GetProperty("ip").GetString()!);
            int port = data.GetProperty("port").GetInt32();
            int ssrc = data.GetProperty("ssrc").GetInt32();

            JsonElement modesArray = data.GetProperty("modes");
            string[] modes = new string[modesArray.GetArrayLength()];
            for (int i = 0; i < modes.Length; i++)
                modes[i] = modesArray[i].ToString()!;

            log.LogVerbose($"[Ready] ssrc = {ssrc}, port = {port}");

            // Notify
            await ReadyQueue.AddAsync(new VoiceReadyEventArgs(ip, port, ssrc, modes));
        }

        [Payload(VoiceOPCode.Resumed)]
        async Task HandleResumedPayload(JsonElement payload, JsonElement data)
        {
            log.LogVerbose("[Resumed] Resume successful.");

            await ResumedQueue.AddAsync(null);
        }

        [Payload(VoiceOPCode.Hello)]
        async Task HandleHelloPayload(JsonElement payload, JsonElement data)
        {
            int heartbeatInterval = (int)data.GetProperty("heartbeat_interval").GetDouble();

            log.LogVerbose($"[Hello] heartbeat_interval = {heartbeatInterval}ms");

            await HelloQueue.AddAsync(heartbeatInterval);

            // Start heartbeat loop
            heartbeatCancellationSource = new CancellationTokenSource();
        }

        [Payload(VoiceOPCode.SessionDescription)]
        async Task HandleSessionDescription(JsonElement payload, JsonElement data)
        {
            JsonElement secretKey = data.GetProperty("secret_key");
            byte[] key = new byte[secretKey.GetArrayLength()];
            for (int i = 0; i < key.Length; i++)
                key[i] = secretKey[i].GetByte();

            string mode = data.GetProperty("mode").GetString()!;

            log.LogVerbose($"[SessionDescription] mode = {mode}");

            await SessionDescriptionQueue.AddAsync(new VoiceSessionDescriptionEventArgs(key, mode));
        }

        [Payload(VoiceOPCode.HeartbeatAck)]
        void HandleHeartbeatAck(JsonElement payload, JsonElement data)
        {
            if (data.ValueKind != JsonValueKind.Null)
            {
                uint returnedNonce;

                if (data.ValueKind == JsonValueKind.String)
                    returnedNonce = uint.Parse(data.GetString()!);
                else
                    returnedNonce = data.GetUInt32();

                if (returnedNonce == heartbeatNonce)
                {
                    heartbeatNonce++;
                    receivedHeartbeatAck = true;
                }
            }
        }

        [Payload(VoiceOPCode.Speaking)]
        void HandleSpeaking(JsonElement payload, JsonElement data)
        {
            Snowflake userId = data.GetProperty("user_id").GetSnowflake();
            int ssrc = data.GetProperty("ssrc").GetInt32();
            var speakingFlag = (SpeakingFlag)data.GetProperty("speaking").GetInt32();

            OnUserSpeaking?.Invoke(this, new VoiceSpeakingEventArgs(userId, ssrc, speakingFlag));
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        /// <exception cref="NotSupportedException">Thrown if the given data cannot be serialized as JSON.</exception>
        Task SendPayload(VoiceOPCode op, Action<Utf8JsonWriter> builder)
        {
            // TODO: There must be a more memory efficient way of doing this

            // Create payload bytes
            using var stream = new MemoryStream();
            using var writer = new Utf8JsonWriter(stream);

            writer.WriteStartObject();

            writer.WriteNumber("op", (int)op);
            writer.WritePropertyName("d");
            builder(writer);

            writer.WriteEndObject();

            writer.Flush();

            byte[] payload = stream.ToArray();

            return SendAsync(payload);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        Task SendHeartbeatPayload()
        {
            return SendPayload(VoiceOPCode.Heartbeat, writer => writer.WriteNumberValue(heartbeatNonce));
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendIdentifyPayload(Snowflake serverId, Snowflake userId, string sessionId, string token)
        {
            void BuildPayload(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                writer.WriteSnowflake("server_id", serverId);
                writer.WriteSnowflake("user_id", userId);
                writer.WriteString("session_id", sessionId);
                writer.WriteString("token", token);

                writer.WriteEndObject();
            }

            log.LogVerbose("[Identify] Sending payload...");
            return SendPayload(VoiceOPCode.Identify, BuildPayload);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendResumePayload(Snowflake serverId, string sessionId, string token)
        {
            void BuildPayload(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                writer.WriteSnowflake("server_id", serverId);
                writer.WriteString("session_id", sessionId);
                writer.WriteString("token", token);

                writer.WriteEndObject();
            }

            log.LogVerbose("[Resume] Sending payload...");
            return SendPayload(VoiceOPCode.Resume, BuildPayload);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendSelectProtocolPayload(string ip, int port, string encryptionMode)
        {
            void BuildPayload(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                writer.WriteString("protocol", "udp");

                writer.WriteStartObject("data");
                writer.WriteString("ip", ip);
                writer.WriteNumber("port", port);
                writer.WriteString("mode", encryptionMode);
                writer.WriteEndObject();

                writer.WriteEndObject();
            }

            log.LogVerbose($"[SelectProtocol] Sending to {ip}:{port}...");
            return SendPayload(VoiceOPCode.SelectProtocol, BuildPayload);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendSpeakingPayload(SpeakingFlag flags, int ssrc)
        {
            void BuildPayload(Utf8JsonWriter writer)
            {
                writer.WriteStartObject();

                writer.WriteNumber("speaking", (int)flags);
                writer.WriteNumber("delay", 0);
                writer.WriteNumber("ssrc", ssrc);

                writer.WriteEndObject();
            }

            return SendPayload(VoiceOPCode.Speaking, BuildPayload);
        }
    }
}

#pragma warning restore IDE0060 // Remove unused parameter
#pragma warning restore IDE0051 // Remove unused private members
