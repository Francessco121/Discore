using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Updates the status of the bot user.
        /// <para>
        /// Note: If this method is called more than 5 times per minute, 
        /// it will block until the remaining time has passed!!
        /// </para>
        /// </summary>
        /// <param name="game">Either null, or an object with one key "name", representing the name of the game being played.</param>
        /// <param name="idleSince">Unix time (in milliseconds) of when the client went idle, or null if the client is not idle.</param>
        void UpdateStatus(string game = null, int? idleSince = null);

        /// <summary>
        /// Requests guild members from the Discord API, this can be used to retrieve
        /// offline members in a guild that is considered "large". "Large" guilds
        /// will not automatically have the offline members available.
        /// <para>
        /// Members requested here will be returned via the callback and available in the cache.
        /// </para>
        /// </summary>
        /// <param name="callback">Action to be invoked if the members are successfully retrieved.</param>
        /// <param name="guildId">The if of the guild to retrieve members from.</param>
        /// <param name="query">String that the username starts with, or an empty string to return all members.</param>
        /// <param name="limit">Maximum number of members to retrieve or 0 to request all members matched.</param>
        void RequestGuildMembers(Action<IReadOnlyList<DiscordGuildMember>> callback, Snowflake guildId,
            string query = "", int limit = 0);
    }
}
