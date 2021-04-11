using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket
{
    public interface IDiscordGateway
    {
        /// <summary>
        /// Gets the shard that is managing this gateway connection.
        /// </summary>
        Shard Shard { get; }

        /// <summary>
        /// Called when a direct message channel is created/opened.
        /// </summary>
        event EventHandler<DMChannelEventArgs>? OnDMChannelCreated;
        /// <summary>
        /// Called when a (text or voice) guild channel is created.
        /// </summary>
        event EventHandler<GuildChannelEventArgs>? OnGuildChannelCreated;
        /// <summary>
        /// Called when a (text or voice) guild channel is updated.
        /// </summary>
        event EventHandler<GuildChannelEventArgs>? OnGuildChannelUpdated;
        /// <summary>
        /// Called when a direct message channel is removed/closed.
        /// </summary>
        event EventHandler<DMChannelEventArgs>? OnDMChannelRemoved;
        /// <summary>
        /// Called when a (text or voice) guild channel is removed.
        /// </summary>
        event EventHandler<GuildChannelEventArgs>? OnGuildChannelRemoved;

        /// <summary>
        /// Called when this application joins a guild.
        /// </summary>
        event EventHandler<GuildEventArgs>? OnGuildCreated;
        /// <summary>
        /// Called when a guild is updated.
        /// </summary>
        event EventHandler<GuildEventArgs>? OnGuildUpdated;
        /// <summary>
        /// Called when this application is removed from a guild.
        /// </summary>
        event EventHandler<GuildEventArgs>? OnGuildRemoved;

        /// <summary>
        /// Called when a known guild that was unavailable becomes available again.
        /// (i.e. when the Gateway discovers guilds that the user is in when connecting).
        /// </summary>
        event EventHandler<GuildEventArgs>? OnGuildAvailable;
        /// <summary>
        /// Called when a known guild to this application becomes unavailable.
        /// This application was NOT removed from the guild.
        /// </summary>
        event EventHandler<GuildEventArgs>? OnGuildUnavailable;

        /// <summary>
        /// Called when a user is banned from a guild.
        /// </summary>
        event EventHandler<GuildUserEventArgs>? OnGuildBanAdded;
        /// <summary>
        /// Called when a user ban is removed from a guild.
        /// </summary>
        event EventHandler<GuildUserEventArgs>? OnGuildBanRemoved;

        /// <summary>
        /// Called when the emojis of a guild are updated.
        /// </summary>
        event EventHandler<GuildEventArgs>? OnGuildEmojisUpdated;

        /// <summary>
        /// Called when the integrations of a guild are updated.
        /// </summary>
        event EventHandler<GuildIntegrationsEventArgs>? OnGuildIntegrationsUpdated;

        /// <summary>
        /// Called when a user joins a guild.
        /// </summary>
        event EventHandler<GuildMemberEventArgs>? OnGuildMemberAdded;
        /// <summary>
        /// Called when a user leaves or gets kicked/banned from a guild.
        /// </summary>
        event EventHandler<GuildMemberEventArgs>? OnGuildMemberRemoved;
        /// <summary>
        /// Called when a member is updated for a specific guild.
        /// </summary>
        event EventHandler<GuildMemberEventArgs>? OnGuildMemberUpdated;
        /// <summary>
        /// Called when members are requested for a guild.
        /// </summary>
        event EventHandler<GuildMemberChunkEventArgs>? OnGuildMembersChunk;

        /// <summary>
        /// Called when a role is added to a guild.
        /// </summary>
        event EventHandler<GuildRoleEventArgs>? OnGuildRoleCreated;
        /// <summary>
        /// Called when a guild role is updated.
        /// </summary>
        event EventHandler<GuildRoleEventArgs>? OnGuildRoleUpdated;
        /// <summary>
        /// Called when a role is removed from a guild.
        /// </summary>
        event EventHandler<GuildRoleEventArgs>? OnGuildRoleDeleted;

        /// <summary>
        /// Called when a message is pinned or unpinned from a channel.
        /// </summary>
        event EventHandler<ChannelPinsUpdateEventArgs>? OnChannelPinsUpdated;

        /// <summary>
        /// Called when a message is created (either from a DM or guild text channel).
        /// </summary>
        event EventHandler<MessageEventArgs>? OnMessageCreated;
        /// <summary>
        /// Called when a message is updated.
        /// <para>
        /// Message contained in this event is only partially filled out!
        /// The only guaranteed field is the channel the message was sent in.
        /// </para>
        /// </summary>
        event EventHandler<MessageUpdateEventArgs>? OnMessageUpdated;
        /// <summary>
        /// Called when a message is deleted.
        /// </summary>
        event EventHandler<MessageDeleteEventArgs>? OnMessageDeleted;
        /// <summary>
        /// Called when someone reacts to a message.
        /// </summary>
        event EventHandler<MessageReactionEventArgs>? OnMessageReactionAdded;
        /// <summary>
        /// Called when a reaction is removed from a message.
        /// </summary>
        event EventHandler<MessageReactionEventArgs>? OnMessageReactionRemoved;
        /// <summary>
        /// Called when all reactions are removed from a message at once.
        /// </summary>
        event EventHandler<MessageReactionRemoveAllEventArgs>? OnMessageAllReactionsRemoved;

        /// <summary>
        /// Called when a webhook is updated.
        /// </summary>
        event EventHandler<WebhooksUpdateEventArgs>? OnWebhookUpdated;

        /// <summary>
        /// Called when the presence of a member in a guild is updated.
        /// </summary>
        event EventHandler<PresenceEventArgs>? OnPresenceUpdated;

        /// <summary>
        /// Called when a user starts typing.
        /// </summary>
        event EventHandler<TypingStartEventArgs>? OnTypingStarted;

        /// <summary>
        /// Called when a user is updated.
        /// </summary>
        event EventHandler<UserEventArgs>? OnUserUpdated;

        /// <summary>
        /// Called when someone joins/leaves/moves voice channels.
        /// </summary>
        event EventHandler<VoiceStateEventArgs>? OnVoiceStateUpdated;

        /// <summary>
        /// Updates the status of the bot user.
        /// <para>Note: This method can only be called 5 times per minute and will wait if this is exceeded.</para>
        /// <para>
        /// Note: This method will also throw an <see cref="OperationCanceledException"/> if the Gateway's shard is stopped while sending.
        /// </para>
        /// </summary>
        /// <param name="options">Options for the new status.</param>
        /// <param name="cancellationToken">A token used to cancel the update.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the Gateway's shard has not been fully started.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the Gateway's shard has been disposed.</exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the cancellation token is cancelled or the Gateway's shard is stopped while sending.
        /// </exception>
        /// <remarks>
        /// This method will wait until the underlying Gateway connection is ready as well as retry if the connection 
        /// closes unexpectedly until the given cancellation token is cancelled or the Gateway's shard is stopped.
        /// </remarks>
        Task UpdateStatusAsync(StatusOptions options, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Requests guild members from the Discord API, this can be used to retrieve offline members in a guild that is considered 
        /// "large". "Large" guilds will not automatically have the offline members available.
        /// <para>
        /// Members requested here will be available through the <see cref="OnGuildMembersChunk"/> event.
        /// </para>
        /// <para>
        /// Note: This method will also throw an <see cref="OperationCanceledException"/> if the Gateway's shard is stopped while sending.
        /// </para>
        /// </summary>
        /// <param name="guildId">The ID of the guild to retrieve members from.</param>
        /// <param name="query">Case-insensitive string that the username starts with, or an empty string to request all members.</param>
        /// <param name="limit">Maximum number of members to retrieve or 0 to request all members matched.</param>
        /// <param name="cancellationToken">A token used to cancel the request.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="query"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the Gateway's shard has not been fully started.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the Gateway's shard has been disposed.</exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the cancellation token is cancelled or the Gateway's shard is stopped while sending.
        /// </exception>
        /// <remarks>
        /// This method will wait until the underlying Gateway connection is ready as well as retry if the connection 
        /// closes unexpectedly until the given cancellation token is cancelled or the Gateway's shard is stopped.
        /// </remarks>
        Task RequestGuildMembersAsync(Snowflake guildId, string query = "", int limit = 0, CancellationToken? cancellationToken = null);
    }
}
