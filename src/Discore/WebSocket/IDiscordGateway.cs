using System;

namespace Discore.WebSocket
{
    public interface IDiscordGateway
    {
        Shard Shard { get; }

        /// <summary>
        /// Called when a direct message channel is created/opened.
        /// </summary>
        event EventHandler<DMChannelEventArgs> OnDMChannelCreated;
        /// <summary>
        /// Called when a (text or voice) guild channel is created.
        /// </summary>
        event EventHandler<GuildChannelEventArgs> OnGuildChannelCreated;
        /// <summary>
        /// Called when a (text or voice) guild channel is updated.
        /// </summary>
        event EventHandler<GuildChannelEventArgs> OnGuildChannelUpdated;
        /// <summary>
        /// Called when a direct message channel is removed/closed.
        /// </summary>
        event EventHandler<DMChannelEventArgs> OnDMChannelRemoved;
        /// <summary>
        /// Called when a (text or voice) guild channel is removed.
        /// </summary>
        event EventHandler<GuildChannelEventArgs> OnGuildChannelRemoved;

        /// <summary>
        /// Called when this application discovers a guild it is in or joins one.
        /// </summary>
        event EventHandler<GuildEventArgs> OnGuildCreated;
        /// <summary>
        /// Called when a guild is updated.
        /// </summary>
        event EventHandler<GuildEventArgs> OnGuildUpdated;
        /// <summary>
        /// Called when this application is removed from a guild.
        /// </summary>
        event EventHandler<GuildEventArgs> OnGuildRemoved;

        /// <summary>
        /// Called when a known guild to this application becomes unavailable.
        /// This application was NOT removed from the guild.
        /// </summary>
        event EventHandler<GuildEventArgs> OnGuildUnavailable;

        /// <summary>
        /// Called when a user is banned from a guild.
        /// </summary>
        event EventHandler<GuildUserEventArgs> OnGuildBanAdded;
        /// <summary>
        /// Called when a user ban is removed from a guild.
        /// </summary>
        event EventHandler<GuildUserEventArgs> OnGuildBanRemoved;

        /// <summary>
        /// Called when the emojis of a guild are updated.
        /// </summary>
        event EventHandler<GuildEventArgs> OnGuildEmojisUpdated;

        /// <summary>
        /// Called when the integrations of a guild are updated.
        /// </summary>
        event EventHandler<GuildEventArgs> OnGuildIntegrationsUpdated;

        /// <summary>
        /// Called when a user joins a guild.
        /// </summary>
        event EventHandler<GuildMemberEventArgs> OnGuildMemberAdded;
        /// <summary>
        /// Called when a user leaves or gets kicked/banned from a guild.
        /// </summary>
        event EventHandler<GuildMemberEventArgs> OnGuildMemberRemoved;
        /// <summary>
        /// Called when a member is updated for a specific guild.
        /// </summary>
        event EventHandler<GuildMemberEventArgs> OnGuildMemberUpdated;

        /// <summary>
        /// Called when a role is added to a guild.
        /// </summary>
        event EventHandler<GuildRoleEventArgs> OnGuildRoleCreated;
        /// <summary>
        /// Called when a guild role is updated.
        /// </summary>
        event EventHandler<GuildRoleEventArgs> OnGuildRoleUpdated;
        /// <summary>
        /// Called when a role is removed from a guild.
        /// </summary>
        event EventHandler<GuildRoleEventArgs> OnGuildRoleDeleted;

        /// <summary>
        /// Called when a message is created (either from a DM or guild text channel).
        /// </summary>
        event EventHandler<MessageEventArgs> OnMessageCreated;
        /// <summary>
        /// Called when a message is updated.
        /// <para>
        /// Message contained in this event is only partially filled out!
        /// The only guaranteed field is the channel the message was sent in.
        /// </para>
        /// </summary>
        event EventHandler<MessageUpdateEventArgs> OnMessageUpdated;
        /// <summary>
        /// Called when a message is deleted.
        /// </summary>
        event EventHandler<MessageDeleteEventArgs> OnMessageDeleted;
        /// <summary>
        /// Called when someone reacts to a message.
        /// </summary>
        event EventHandler<MessageReactionEventArgs> OnMessageReactionAdded;
        /// <summary>
        /// Called when a reaction is removed from a message.
        /// </summary>
        event EventHandler<MessageReactionEventArgs> OnMessageReactionRemoved;

        /// <summary>
        /// Called when the presence of a member in a guild is updated.
        /// </summary>
        event EventHandler<GuildMemberEventArgs> OnPresenceUpdated;

        /// <summary>
        /// Called when a user starts typing.
        /// </summary>
        event EventHandler<TypingStartEventArgs> OnTypingStarted;

        /// <summary>
        /// Called when a user is updated.
        /// </summary>
        event EventHandler<UserEventArgs> OnUserUpdated;
    }
}
