using Discore.WebSocket;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.Voice
{
    /// <summary>
    /// Creates a Gateway voice bridge from an <see cref="IDiscordGateway"/> implementation.
    /// </summary>
    public class DefaultGatewayVoiceBridge : IGatewayVoiceBridge, IDisposable
    {
        public event EventHandler<BridgeVoiceStateUpdateEventArgs>? OnVoiceStateUpdate;
        public event EventHandler<BridgeVoiceServerUpdateEventArgs>? OnVoiceServerUpdate;

        bool isDisposed;

        readonly IDiscordGateway gateway;

        /// <summary>
        /// Creates a Gateway voice bridge from a Gateway connection.
        /// </summary>
        /// <param name="gateway">The Gateway connection.</param>
        public DefaultGatewayVoiceBridge(IDiscordGateway gateway)
        {
            this.gateway = gateway;

            gateway.OnVoiceStateUpdate += Gateway_OnVoiceStateUpdate;
            gateway.OnVoiceServerUpdate += Gateway_OnVoiceServerUpdate;
        }

        public async Task UpdateVoiceStateAsync(
            Snowflake guildId,
            Snowflake? channelId,
            bool isMute = false,
            bool isDeaf = false,
            CancellationToken? cancellationToken = null)
        {
            if (isDisposed)
                throw new ObjectDisposedException(GetType().FullName, "This default Gateway voice bridge has been disposed.");

            try
            {
                await gateway.UpdateVoiceStateAsync(guildId, channelId, isMute, isDeaf, cancellationToken);
            }
            catch (ObjectDisposedException ex)
            {
                throw new InvalidOperationException("The Gateway connection is no longer valid.", ex);
            }
        }

        public void Dispose()
        {
            if (!isDisposed)
            {
                isDisposed = true;

                gateway.OnVoiceStateUpdate -= Gateway_OnVoiceStateUpdate;
                gateway.OnVoiceServerUpdate -= Gateway_OnVoiceServerUpdate;
            }
        }

        private void Gateway_OnVoiceStateUpdate(object? sender, VoiceStateUpdateEventArgs e)
        {
            OnVoiceStateUpdate?.Invoke(this, new BridgeVoiceStateUpdateEventArgs(e.VoiceState));
        }

        private void Gateway_OnVoiceServerUpdate(object? sender, VoiceServerUpdateEventArgs e)
        {
            OnVoiceServerUpdate?.Invoke(this, new BridgeVoiceServerUpdateEventArgs(e.VoiceServer));
        }
    }
}
