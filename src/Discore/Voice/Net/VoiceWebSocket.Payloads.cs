using Discore.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice.Net
{
    partial class VoiceWebSocket
    {
		[Payload(VoiceOPCode.Ready)]
		void HandleReadyPayload(DiscordApiData payload, DiscordApiData data)
        {
            int port = data.GetInteger("port").Value;
            int ssrc = data.GetInteger("ssrc").Value;

            IList<DiscordApiData> modesArray = data.GetArray("modes");
            string[] modes = new string[modesArray.Count];
            for (int i = 0; i < modes.Length; i++)
                modes[i] = modesArray[i].ToString();

            heartbeatInterval = data.GetInteger("heartbeat_interval").Value;

            log.LogVerbose($"[Ready] ssrc = {ssrc}, port = {port}");

			// Start heartbeat loop
            heartbeatCancellationSource = new CancellationTokenSource();
            heartbeatTask = HeartbeatLoop();

			// Notify
            OnReady?.Invoke(this, new VoiceReadyEventArgs(port, ssrc, modes));
        }

		[Payload(VoiceOPCode.SessionDescription)]
		void HandleSessionDescription(DiscordApiData payload, DiscordApiData data)
        {
            IList<DiscordApiData> secretKey = data.GetArray("secret_key");
            byte[] key = new byte[secretKey.Count];
            for (int i = 0; i < key.Length; i++)
                key[i] = (byte)secretKey[i].ToInteger();

            string mode = data.GetString("mode");

            log.LogVerbose($"[SessionDescription] mode = {mode}");

            OnSessionDescription?.Invoke(this, new VoiceSessionDescriptionEventArgs(key, mode));
        }

        [Payload(VoiceOPCode.Heartbeat)]
        void HandleHeartbeatAck(DiscordApiData payload, DiscordApiData data)
        {
            receivedHeartbeatAck = true;
        }

        [Payload(VoiceOPCode.Speaking)]
        void HandleSpeaking(DiscordApiData payload, DiscordApiData data)
        {
            Snowflake userId = data.GetSnowflake("user_id").Value;
            int ssrc = data.GetInteger("ssrc").Value;
            bool isSpeaking = data.GetBoolean("speaking").Value;

            OnUserSpeaking?.Invoke(this, new VoiceSpeakingEventArgs(userId, ssrc, isSpeaking));
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        /// <exception cref="JsonWriterException">Thrown if the given data cannot be serialized as JSON.</exception>
        Task SendPayload(VoiceOPCode op, DiscordApiData data)
        {
            DiscordApiData payload = new DiscordApiData();
            payload.Set("op", (int)op);
            payload.Set("d", data);

            return SendAsync(payload);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        Task SendHeartbeatPayload()
        {
            return SendPayload(VoiceOPCode.Heartbeat, null);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendIdentifyPayload(Snowflake serverId, Snowflake userId, string sessionId, string token)
        {
            DiscordApiData data = new DiscordApiData();
            data.Set("server_id", serverId);
            data.Set("user_id", userId);
            data.Set("session_id", sessionId);
            data.Set("token", token);

            log.LogVerbose("[Identify] Sending payload...");
            return SendPayload(VoiceOPCode.Identify, data);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendSelectProtocolPayload(string ip, int port, string encryptionMode)
        {
            DiscordApiData selectProtocol = new DiscordApiData();
            selectProtocol.Set("protocol", "udp");
            DiscordApiData data = selectProtocol.Set("data", DiscordApiData.CreateContainer());
            data.Set("address", ip);
            data.Set("port", port);
            data.Set("mode", encryptionMode);

            log.LogVerbose($"[SelectProtocol] Sending to {ip}:{port}...");
            return SendPayload(VoiceOPCode.SelectProtocol, selectProtocol);
        }

        /// <exception cref="DiscordWebSocketException">Thrown if the payload fails to send because of a WebSocket error.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the socket is not connected.</exception>
        public Task SendSpeakingPayload(bool speaking)
        {
            DiscordApiData data = new DiscordApiData();
            data.Set("speaking", speaking);
            data.Set("delay", 0); // TODO: is this needed?

            return SendPayload(VoiceOPCode.Speaking, data);
        }
    }
}
