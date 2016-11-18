using System;

namespace Discore.WebSocket.Net
{
    public interface IDiscordGateway
    {
        Shard Shard { get; }

        event EventHandler<DMChannelEventArgs> OnDMChannelCreated;
        event EventHandler<GuildChannelEventArgs> OnGuildChannelCreated;
        event EventHandler<GuildChannelEventArgs> OnGuildChannelUpdated;
        event EventHandler<DMChannelEventArgs> OnDMChannelRemoved;
        event EventHandler<GuildChannelEventArgs> OnGuildChannelRemoved;

        event EventHandler<GuildEventArgs> OnGuildCreated;
        event EventHandler<GuildEventArgs> OnGuildUpdated;
        event EventHandler<GuildEventArgs> OnGuildRemoved;

        event EventHandler<GuildEventArgs> OnGuildUnavailable;

        event EventHandler<GuildUserEventArgs> OnGuildBanAdded;
        event EventHandler<GuildUserEventArgs> OnGuildBanRemoved;

        event EventHandler<GuildEventArgs> OnEmojisUpdated;

        event EventHandler<GuildEventArgs> OnGuildIntegrationsUpdated;

        event EventHandler<GuildMemberEventArgs> OnGuildMemberAdded;
        event EventHandler<GuildMemberEventArgs> OnGuildMemberRemoved;
        event EventHandler<GuildMemberEventArgs> OnGuildMemberUpdated;

        event EventHandler<GuildRoleEventArgs> OnGuildRoleCreated;
        event EventHandler<GuildRoleEventArgs> OnGuildRoleUpdated;
        event EventHandler<GuildRoleEventArgs> OnGuildRoleDeleted;

        event EventHandler<MessageEventArgs> OnMessageCreated;
        event EventHandler<MessageEventArgs> OnMessageUpdated;
        event EventHandler<MessageEventArgs> OnMessageDeleted;

        event EventHandler<GuildMemberEventArgs> OnPresenceUpdated;

        event EventHandler<TypingStartEventArgs> OnTypingStarted;

        event EventHandler<UserEventArgs> OnUserUpdated;
    }
}
