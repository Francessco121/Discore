using Discore.Net;

namespace Discore
{
    public interface IDiscordClient : ICacheContainer
    {
        IDiscordRestClient Rest { get; }
        IDiscordGateway Gateway { get; }
    }
}
