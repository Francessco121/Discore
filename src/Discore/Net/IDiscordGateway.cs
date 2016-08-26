using Discore.Audio;

namespace Discore.Net
{
    /// <summary>
    /// Provides interaction with the Discord gateway API.
    /// </summary>
    public interface IDiscordGateway
    {
        /// <summary>
        /// Creates a voice connection to the specified <see cref="DiscordGuildChannel"/>.
        /// </summary>
        /// <param name="channel">The guild voice channel to connect to.</param>
        /// <returns>The voice client interface for the created connection.</returns>
        DiscordVoiceClient ConnectToVoice(DiscordGuildChannel channel);

        /// <summary>
        /// Disconnects the <see cref="DiscordVoiceClient"/> currently connected to the specified <see cref="DiscordGuild"/>.
        /// </summary>
        /// <param name="guild">The <see cref="DiscordGuild"/> to disconnect from.</param>
        void DisconnectFromVoice(DiscordGuild guild);
    }
}
