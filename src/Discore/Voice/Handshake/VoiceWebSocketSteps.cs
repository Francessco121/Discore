using Discore.Voice.Net;
using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice.Handshake
{
    static class VoiceWebSocketSteps
    {
        public static Task CreateVoiceWebSocket(VoiceConnectionState state, DiscoreLogger log)
        {
            if (state.WebSocket != null)
                throw new InvalidOperationException("[CreateVoiceWebSocket] state.WebSocket must be null!");

            state.WebSocket = new VoiceWebSocket($"VoiceWebSocket:{state.GuildId}");

            log.LogVerbose("Created VoiceWebSocket.");

            return Task.CompletedTask;
        }

        public static async Task ConnectVoiceWebSocket(VoiceConnectionState state, DiscoreLogger log)
        {
            if (state.WebSocket == null)
                throw new InvalidOperationException("[SendVoiceIdentify] state.WebSocket must not be null!");
            if (state.EndPoint == null)
                throw new InvalidOperationException("[ConnectVoiceWebSocket] state.EndPoint must not be null!");

            // Build WebSocket URI
            Uri uri = new Uri($"wss://{state.EndPoint}?v={VoiceWebSocket.GATEWAY_VERSION}");

            log.LogVerbose($"[ConnectVoiceWebSocket] Connecting WebSocket to {uri}...");

            // Connect
            try
            {
                await state.WebSocket.ConnectAsync(uri, CancellationToken.None)
                    .ConfigureAwait(false);
            }
            catch (WebSocketException ex)
            {
                log.LogError($"[ConnectVoiceWebSocket] Failed to connect to {uri}: " +
                    $"code = {ex.WebSocketErrorCode}, error = {ex}");
                throw;
            }

            log.LogVerbose("Connected WebSocket.");
        }

        public static Task ReceiveVoiceHello(VoiceConnectionState state, DiscoreLogger log)
        {
            if (state.WebSocket == null)
                throw new InvalidOperationException("[SendVoiceIdentify] state.WebSocket must not be null!");

            CancellationTokenSource tokenSource = new CancellationTokenSource();
            tokenSource.CancelAfter(10 * 1000);

            return Task.Run(() =>
            {
                state.HeartbeatInterval = state.WebSocket.HelloQueue.Take(tokenSource.Token);
            });
        }

        public static async Task SendVoiceIdentify(VoiceConnectionState state, DiscoreLogger log)
        {
            if (state.WebSocket == null)
                throw new InvalidOperationException("[SendVoiceIdentify] state.WebSocket must not be null!");
            if (state.Shard == null)
                throw new InvalidOperationException("[SendVoiceIdentify] state.Shard must not be null!");
            if (!state.Shard.UserId.HasValue)
                throw new InvalidOperationException("[SendVoiceIdentify] state.Shard.UserId must not be null!");
            if (state.VoiceState == null)
                throw new InvalidOperationException("[SendVoiceIdentify] state.VoiceState must not be null!");
            if (state.VoiceState.SessionId == null)
                throw new InvalidOperationException("[SendVoiceIdentify] state.VoiceState.SessionId must not be null!");
            if (state.Token == null)
                throw new InvalidOperationException("[SendVoiceIdentify] state.Token must not be null!");

            await state.WebSocket.SendIdentifyPayload(state.GuildId, state.Shard.UserId.Value, 
                state.VoiceState.SessionId, state.Token)
                .ConfigureAwait(false);
        }
    }
}
