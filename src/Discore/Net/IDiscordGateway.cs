using Discore.Audio;

namespace Discore.Net
{
    public interface IDiscordGateway
    {
        DiscordVoiceClient ConnectToVoice(DiscordGuildChannel channel);
        void DisconnectFromVoice(DiscordGuild guild);
    }
}
