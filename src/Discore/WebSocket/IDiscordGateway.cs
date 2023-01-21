using Discore.Voice;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Discore.WebSocket
{
    // Note: Gateway event names should follow the pattern: On{{EventName}}

    public interface IDiscordGateway
    {
        /// <summary>
        /// Gets the shard that is managing this gateway connection.
        /// </summary>
        Shard Shard { get; }

        /// <summary>
        /// Called when the Gateway establishes a new session.
        /// </summary>
        event EventHandler<ReadyEventArgs>? OnReady;

        /// <summary>
        /// Called when a channel is created, relavent to the current application.
        /// </summary>
        event EventHandler<ChannelCreateEventArgs>? OnChannelCreate;
        /// <summary>
        /// Called when a channel is updated.
        /// </summary>
        event EventHandler<ChannelUpdateEventArgs>? OnChannelUpdate;
        /// <summary>
        /// Called when a channel is deleted, relavent to the current application.
        /// </summary>
        event EventHandler<ChannelDeleteEventArgs>? OnChannelDelete;

        /// <summary>
        /// Called when this application joins a guild or when a known guild that was
        /// unavailable becomes available again (i.e. when the Gateway discovers 
        /// guilds that the user is in when connecting).
        /// </summary>
        event EventHandler<GuildCreateEventArgs>? OnGuildCreate;
        /// <summary>
        /// Called when a guild is updated.
        /// </summary>
        event EventHandler<GuildUpdateEventArgs>? OnGuildUpdate;
        /// <summary>
        /// Called when this application is removed from a guild or when a guild
        /// becomes unavailable (if the guild became unavailable, this application
        /// was NOT removed from the guild).
        /// </summary>
        event EventHandler<GuildDeleteEventArgs>? OnGuildDelete;

        /// <summary>
        /// Called when a user is banned from a guild.
        /// </summary>
        event EventHandler<GuildBanAddEventArgs>? OnGuildBanAdd;
        /// <summary>
        /// Called when a user ban is removed from a guild (i.e. they were unbanned).
        /// </summary>
        event EventHandler<GuildBanRemoveEventArgs>? OnGuildBanRemove;

        /// <summary>
        /// Called when the emojis of a guild are updated.
        /// </summary>
        event EventHandler<GuildEmojisUpdateEventArgs>? OnGuildEmojisUpdate;

        /// <summary>
        /// Called when the integrations of a guild are updated.
        /// </summary>
        event EventHandler<GuildIntegrationsUpdateEventArgs>? OnGuildIntegrationsUpdate;

        /// <summary>
        /// Called when a user joins a guild.
        /// </summary>
        event EventHandler<GuildMemberAddEventArgs>? OnGuildMemberAdd;
        /// <summary>
        /// Called when a user leaves or gets kicked/banned from a guild.
        /// </summary>
        event EventHandler<GuildMemberRemoveEventArgs>? OnGuildMemberRemove;
        /// <summary>
        /// Called when a member is updated for a specific guild.
        /// </summary>
        event EventHandler<GuildMemberUpdateEventArgs>? OnGuildMemberUpdate;
        /// <summary>
        /// Called when members are requested for a guild.
        /// </summary>
        event EventHandler<GuildMemberChunkEventArgs>? OnGuildMembersChunk;

        /// <summary>
        /// Called when a role is added to a guild.
        /// </summary>
        event EventHandler<GuildRoleCreateEventArgs>? OnGuildRoleCreate;
        /// <summary>
        /// Called when a guild role is updated.
        /// </summary>
        event EventHandler<GuildRoleUpdateEventArgs>? OnGuildRoleUpdate;
        /// <summary>
        /// Called when a role is removed from a guild.
        /// </summary>
        event EventHandler<GuildRoleDeleteEventArgs>? OnGuildRoleDelete;

        /// <summary>
        /// Called when a message is pinned or unpinned from a channel.
        /// </summary>
        event EventHandler<ChannelPinsUpdateEventArgs>? OnChannelPinsUpdate;

        /// <summary>
        /// Called when a message is created (either from a DM or guild text channel).
        /// </summary>
        event EventHandler<MessageCreateEventArgs>? OnMessageCreate;
        /// <summary>
        /// Called when a message is updated.
        /// <para>
        /// Message contained in this event is only partially filled out!
        /// The only guaranteed field is the channel the message was sent in.
        /// </para>
        /// </summary>
        event EventHandler<MessageUpdateEventArgs>? OnMessageUpdate;
        /// <summary>
        /// Called when a message is deleted.
        /// <para/>
        /// If multiple messages are deleted in bulk, this event will be fired for each
        /// individual message.
        /// </summary>
        event EventHandler<MessageDeleteEventArgs>? OnMessageDelete;
        /// <summary>
        /// Called when someone reacts to a message.
        /// </summary>
        event EventHandler<MessageReactionAddEventArgs>? OnMessageReactionAdd;
        /// <summary>
        /// Called when a reaction is removed from a message.
        /// </summary>
        event EventHandler<MessageReactionRemoveEventArgs>? OnMessageReactionRemove;
        /// <summary>
        /// Called when all reactions are removed from a message at once.
        /// </summary>
        event EventHandler<MessageReactionRemoveAllEventArgs>? OnMessageReactionRemoveAll;

        /// <summary>
        /// Called when a webhook is updated.
        /// </summary>
        event EventHandler<WebhooksUpdateEventArgs>? OnWebhookUpdate;

        /// <summary>
        /// Called when the presence of a member in a guild is updated.
        /// </summary>
        event EventHandler<PresenceUpdateEventArgs>? OnPresenceUpdate;

        /// <summary>
        /// Called when a user starts typing.
        /// </summary>
        event EventHandler<TypingStartEventArgs>? OnTypingStart;

        /// <summary>
        /// Called when a user is updated.
        /// </summary>
        event EventHandler<UserUpdateEventArgs>? OnUserUpdate;

        /// <summary>
        /// Called when someone joins/leaves/moves voice channels.
        /// </summary>
        event EventHandler<VoiceStateUpdateEventArgs>? OnVoiceStateUpdate;

        /// <summary>
        /// Called when the voice server for a guild is updated.
        /// </summary>
        /// <remarks>
        /// This is sent when initially connecting to voice and when the current voice 
        /// instance fails over to a new server.
        /// <para/>
        /// Usually, applications do not need to listen for this as <see cref="DiscordVoiceConnection"/> will
        /// handle it for you.
        /// </remarks>
        event EventHandler<VoiceServerUpdateEventArgs>? OnVoiceServerUpdate;

        /// <summary>
        /// Updates the presence/status of the bot user.
        /// </summary>
        /// <param name="options">Options for the new presence.</param>
        /// <param name="cancellationToken">A token used to cancel the update.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if the Gateway's shard has not been fully started.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the Gateway's shard has been disposed.</exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the cancellation token is cancelled or the Gateway's shard is stopped while sending.
        /// </exception>
        /// <remarks>
        /// <para>Note: This method can only be called 5 times per minute and will wait if this is exceeded.</para>
        /// <para>
        /// Note: This method will also throw an <see cref="OperationCanceledException"/> if the Gateway's shard is stopped while sending.
        /// </para>
        /// This method will wait until the underlying Gateway connection is ready as well as retry if the connection 
        /// closes unexpectedly until the given cancellation token is cancelled or the Gateway's shard is stopped.
        /// </remarks>
        Task UpdatePresenceAsync(PresenceOptions options, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Requests guild members from the Discord API, this can be used to retrieve offline members in a guild that is considered 
        /// "large". "Large" guilds will not automatically have the offline members available.
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
        /// <para>
        /// Members requested here will be available through the <see cref="OnGuildMembersChunk"/> event.
        /// </para>
        /// <para>
        /// Note: This method will also throw an <see cref="OperationCanceledException"/> if the Gateway's shard is stopped while sending.
        /// </para>
        /// This method will wait until the underlying Gateway connection is ready as well as retry if the connection 
        /// closes unexpectedly until the given cancellation token is cancelled or the Gateway's shard is stopped.
        /// </remarks>
        Task RequestGuildMembersAsync(Snowflake guildId, string query = "", int limit = 0, CancellationToken? cancellationToken = null);

        /// <summary>
        /// Joins, moves, or disconnects the application from a voice channel.
        /// </summary>
        /// <param name="guildId">
        /// The ID of the guild containing the voice channel. 
        /// An application can only be in one voice channel at a time per guild.
        /// </param>
        /// <param name="channelId">The ID of the voice channel to join/move to or null to disconnect from voice.</param>
        /// <param name="isMute">Whether the application is self-mute.</param>
        /// <param name="isDeaf">Whether the application is self-deafened.</param>
        /// <param name="cancellationToken">A token used to cancel the request.</param>
        /// <exception cref="InvalidOperationException">Thrown if the Gateway's shard has not been fully started.</exception>
        /// <exception cref="ObjectDisposedException">Thrown if the Gateway's shard has been disposed.</exception>
        /// <exception cref="OperationCanceledException">
        /// Thrown if the cancellation token is cancelled or the Gateway's shard is stopped while sending.
        /// </exception>
        /// <remarks>
        /// Normally, this method does not need to be called directly as <see cref="DiscordVoiceConnection"/>s will call this for you.
        /// <para/>
        /// An application can only be in one voice channel at a time per guild.
        /// <para/>
        /// This method will wait until the underlying Gateway connection is ready as well as retry if the connection 
        /// closes unexpectedly until the given cancellation token is cancelled or the Gateway's shard is stopped.
        /// </remarks>
        Task UpdateVoiceStateAsync(Snowflake guildId, Snowflake? channelId, bool isMute = false, bool isDeaf = false,
            CancellationToken? cancellationToken = null);
    }
}
