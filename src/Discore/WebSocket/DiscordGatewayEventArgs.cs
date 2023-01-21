using Discore.Voice;
using System;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    // Note: Event names should follow the pattern {{EventName}}EventArgs
    // Example: MESSAGE_DELETE -> MessageDeleteEventArgs

    public abstract class DiscordGatewayEventArgs : EventArgs
    {
        /// <summary>
        /// The shard of the Gateway connection that fired the event.
        /// </summary>
        public Shard Shard { get; }

        internal DiscordGatewayEventArgs(Shard shard)
        {
            Shard = shard;
        }
    }

    public class ReadyEventArgs : DiscordGatewayEventArgs
    {
        public class ShardInfo
        {
            /// <summary>
            /// The shard ID that is associated with the Gateway connection.
            /// </summary>
            public int Id { get; }

            /// <summary>
            /// The total number of shards provided to the Gateway connection.
            /// </summary>
            public int Total { get; }

            internal ShardInfo(int id, int total)
            {
                Id = id;
                Total = total;
            }
        }

        /// <summary>
        /// The user of the bot application.
        /// </summary>
        public DiscordUser User { get; }

        /// <summary>
        /// The IDs of every guild that the bot is in.
        /// </summary>
        public Snowflake[] GuildIds { get; }

        /// <summary>
        /// Information about the shard.
        /// <para/>
        /// Will be null if no shard information was provided to the Gateway connection
        /// (i.e. the total number of shards was 1).
        /// </summary>
        public ShardInfo? ShardInformation { get; }

        internal ReadyEventArgs(Shard shard, DiscordUser user, Snowflake[] guildIds, int? shardId, int? totalShards)
            : base(shard)
        {
            User = user;
            GuildIds = guildIds;

            if (shardId != null && totalShards != null)
                ShardInformation = new ShardInfo(shardId.Value, totalShards.Value);
        }
    }

    public class UserUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The user that was updated (including the updated information).
        /// </summary>
        public DiscordUser User { get; }

        internal UserUpdateEventArgs(Shard shard, DiscordUser user)
            : base(shard)
        {
            User = user;
        }
    }

    public class TypingStartEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the user that started typing.
        /// </summary>
        public Snowflake UserId { get; }
        /// <summary>
        /// The ID of the text channel that the user starting typing in.
        /// </summary>
        public Snowflake ChannelId { get; }
        /// <summary>
        /// Unix time in seconds when the typing started.
        /// </summary>
        public int Timestamp { get; }

        // TODO: add guild_id, member

        internal TypingStartEventArgs(Shard shard, Snowflake userId, Snowflake channelId, int timestamp)
            : base(shard)
        {
            UserId = userId;
            ChannelId = channelId;
            Timestamp = timestamp;
        }
    }

    public class GuildMemberAddEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild that the user joined.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// The guild-specific user information for the new member.
        /// </summary>
        public DiscordGuildMember Member { get; }

        internal GuildMemberAddEventArgs(Shard shard, Snowflake guildId, DiscordGuildMember member)
            : base(shard)
        {
            GuildId = guildId;
            Member = member;
        }
    }

    public class GuildMemberRemoveEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild that the user left.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// The user that left the guild.
        /// </summary>
        public DiscordUser User { get; }

        internal GuildMemberRemoveEventArgs(Shard shard, Snowflake guildId, DiscordUser user)
            : base(shard)
        {
            GuildId = guildId;
            User = user;
        }
    }

    public class GuildMemberUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild the member is in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// Partial updated information for the member.
        /// </summary>
        public DiscordPartialGuildMember PartialMember { get; }

        internal GuildMemberUpdateEventArgs(Shard shard, Snowflake guildId, DiscordPartialGuildMember partialMember)
            : base(shard)
        {
            GuildId = guildId;
            PartialMember = partialMember;
        }
    }

    public class PresenceUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild the member is in.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// The presence state of the user.
        /// </summary>
        public DiscordUserPresence Presence { get; }

        internal PresenceUpdateEventArgs(Shard shard, Snowflake guildId, DiscordUserPresence presence)
            : base(shard)
        {
            GuildId = guildId;
            Presence = presence;
        }
    }

    public class GuildMemberChunkEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild that the members are in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// A list of all members included in the chunk.
        /// </summary>
        public DiscordGuildMember[] Members { get; }

        internal GuildMemberChunkEventArgs(Shard shard, Snowflake guildId, DiscordGuildMember[] members)
            : base(shard)
        {
            GuildId = guildId;
            Members = members;
        }
    }

    public class GuildBanAddEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild the user was banned from.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// The user that was banned.
        /// </summary>
        public DiscordUser User { get; }

        internal GuildBanAddEventArgs(Shard shard, Snowflake guildId, DiscordUser user)
            : base(shard)
        {
            GuildId = guildId;
            User = user;
        }
    }

    public class GuildBanRemoveEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild the user was unbanned from.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// The user that was unbanned.
        /// </summary>
        public DiscordUser User { get; }

        internal GuildBanRemoveEventArgs(Shard shard, Snowflake guildId, DiscordUser user)
            : base(shard)
        {
            GuildId = guildId;
            User = user;
        }
    }

    public class GuildRoleCreateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild that the role was created in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// The created role.
        /// </summary>
        public DiscordRole Role { get; }

        internal GuildRoleCreateEventArgs(Shard shard, Snowflake guildId, DiscordRole role)
            : base(shard)
        {
            GuildId = guildId;
            Role = role;
        }
    }

    public class GuildRoleUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild that the role is in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// The updated role.
        /// </summary>
        public DiscordRole Role { get; }

        internal GuildRoleUpdateEventArgs(Shard shard, Snowflake guildId, DiscordRole role)
            : base(shard)
        {
            GuildId = guildId;
            Role = role;
        }
    }

    public class GuildRoleDeleteEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild that the role was in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// The ID of the deleted role.
        /// </summary>
        public Snowflake RoleId { get; }

        internal GuildRoleDeleteEventArgs(Shard shard, Snowflake guildId, Snowflake roleId)
            : base(shard)
        {
            GuildId = guildId;
            RoleId = roleId;
        }
    }

    public class GuildCreateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Whether the guild just became available.
        /// <para/>
        /// If false, the application just joined the guild for the first time.
        /// </summary>
        public bool BecameAvailable { get; }

        /// <summary>
        /// The guild associated with the event.
        /// </summary>
        public DiscordGuild Guild { get; }

        /// <summary>
        /// Additional metadata about the guild.
        /// </summary>
        public DiscordGuildMetadata GuildMetadata { get; }

        /// <summary>
        /// A list of all guild members.
        /// </summary>
        public IReadOnlyList<DiscordGuildMember> Members { get; }

        /// <summary>
        /// A list of all channels in the guild.
        /// </summary>
        public IReadOnlyList<DiscordGuildChannel> Channels { get; }

        /// <summary>
        /// A list of states of members currently in voice channels.
        /// </summary>
        public IReadOnlyList<DiscordVoiceState> VoiceStates { get; }

        /// <summary>
        /// A list of presence information for each member.
        /// </summary>
        public IReadOnlyList<DiscordUserPresence> Presences { get; }

        internal GuildCreateEventArgs(
            Shard shard,
            bool becameAvailable,
            DiscordGuild guild,
            DiscordGuildMetadata guildMetadata,
            IReadOnlyList<DiscordGuildMember> members,
            IReadOnlyList<DiscordGuildChannel> channels,
            IReadOnlyList<DiscordVoiceState> voiceStates,
            IReadOnlyList<DiscordUserPresence> presences)
            : base(shard)
        {
            BecameAvailable = becameAvailable;
            Guild = guild;
            GuildMetadata = guildMetadata;
            Members = members;
            Channels = channels;
            VoiceStates = voiceStates;
            Presences = presences;
        }
    }

    public class GuildUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The updated guild.
        /// </summary>
        public DiscordGuild Guild { get; }

        internal GuildUpdateEventArgs(Shard shard, DiscordGuild guild)
            : base(shard)
        {
            Guild = guild;
        }
    }

    public class GuildDeleteEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Whether the guild only became unavailable.
        /// <para/>
        /// If true, the application was NOT removed from the guild.
        /// </summary>
        public bool Unavailable { get; }

        internal GuildDeleteEventArgs(Shard shard, Snowflake guildId, bool unavailable)
            : base(shard)
        {
            GuildId = guildId;
            Unavailable = unavailable;
        }
    }

    public class GuildEmojisUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild where the emojis were updated.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// The new list of guild emojis.
        /// </summary>
        public IReadOnlyList<DiscordEmoji> Emojis { get; }

        internal GuildEmojisUpdateEventArgs(Shard shard, Snowflake guildId, IReadOnlyList<DiscordEmoji> emojis)
            : base(shard)
        {
            GuildId = guildId;
            Emojis = emojis;
        }
    }

    public class GuildIntegrationsUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild that had its integrations updated.
        /// </summary>
        public Snowflake GuildId { get; }

        internal GuildIntegrationsUpdateEventArgs(Shard shard, Snowflake guildId)
            : base(shard)
        {
            GuildId = guildId;
        }
    }

    public class ChannelCreateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The created channel.
        /// </summary>
        public DiscordChannel Channel { get; }

        internal ChannelCreateEventArgs(Shard shard, DiscordChannel channel)
            : base(shard)
        {
            Channel = channel;
        }
    }

    public class ChannelUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The updated channel.
        /// </summary>
        public DiscordChannel Channel { get; }

        internal ChannelUpdateEventArgs(Shard shard, DiscordChannel channel)
            : base(shard)
        {
            Channel = channel;
        }
    }

    public class ChannelDeleteEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The deleted channel.
        /// </summary>
        public DiscordChannel Channel { get; }

        internal ChannelDeleteEventArgs(Shard shard, DiscordChannel channel)
            : base(shard)
        {
            Channel = channel;
        }
    }

    public class WebhooksUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild that had a webhook updated.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// The ID of the channel that had a webhook updated.
        /// </summary>
        public Snowflake ChannelId { get; }

        internal WebhooksUpdateEventArgs(Shard shard, Snowflake guildId, Snowflake channelId)
            : base(shard)
        {
            GuildId = guildId;
            ChannelId = channelId;
        }
    }

    public class ChannelPinsUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the text channel that had its pins updated.
        /// </summary>
        public Snowflake ChannelId { get; }
        /// <summary>
        /// The date-time of the newest pin as of this update (or null if there is no longer any pins).
        /// </summary>
        public DateTime? LastPinTimestamp { get; }

        internal ChannelPinsUpdateEventArgs(Shard shard, Snowflake channelId, DateTime? lastPinTimestamp)
            : base(shard)
        {
            ChannelId = channelId;
            LastPinTimestamp = lastPinTimestamp;
        }
    }

    public class MessageCreateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The created message.
        /// </summary>
        public DiscordMessage Message { get; }

        internal MessageCreateEventArgs(Shard shard, DiscordMessage message)
            : base(shard)
        {
            Message = message;
        }
    }

    public class MessageUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// A partial message object representing the changes made to the message.
        /// </summary>
        public DiscordPartialMessage PartialMessage { get; }

        internal MessageUpdateEventArgs(Shard shard, DiscordPartialMessage message)
            : base(shard)
        {
            PartialMessage = message;
        }
    }

    public class MessageDeleteEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the message that was deleted.
        /// </summary>
        public Snowflake MessageId { get; }
        /// <summary>
        /// The ID of the channel the message was deleted from.
        /// </summary>
        public Snowflake ChannelId { get; }

        internal MessageDeleteEventArgs(Shard shard, Snowflake messageId, Snowflake channelId)
            : base(shard)
        {
            MessageId = messageId;
            ChannelId = channelId;
        }
    }

    public class MessageReactionAddEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the message associated with the reaction.
        /// </summary>
        public Snowflake MessageId { get; }
        /// <summary>
        /// The ID of the user who added/removed the reaction.
        /// </summary>
        public Snowflake UserId { get; }
        /// <summary>
        /// The ID of the channel the message affected is in.
        /// </summary>
        public Snowflake ChannelId { get; }
        /// <summary>
        /// The emoji associated with the event.
        /// </summary>
        public DiscordReactionEmoji Emoji { get; }

        internal MessageReactionAddEventArgs(Shard shard, Snowflake messageId, Snowflake channelId,
            Snowflake userId, DiscordReactionEmoji emoji)
            : base(shard)
        {
            MessageId = messageId;
            ChannelId = channelId;
            UserId = userId;
            Emoji = emoji;
        }
    }

    public class MessageReactionRemoveEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the message associated with the reaction.
        /// </summary>
        public Snowflake MessageId { get; }
        /// <summary>
        /// The ID of the user who added/removed the reaction.
        /// </summary>
        public Snowflake UserId { get; }
        /// <summary>
        /// The ID of the channel the message affected is in.
        /// </summary>
        public Snowflake ChannelId { get; }
        /// <summary>
        /// The emoji associated with the event.
        /// </summary>
        public DiscordReactionEmoji Emoji { get; }

        internal MessageReactionRemoveEventArgs(Shard shard, Snowflake messageId, Snowflake channelId,
            Snowflake userId, DiscordReactionEmoji emoji)
            : base(shard)
        {
            MessageId = messageId;
            ChannelId = channelId;
            UserId = userId;
            Emoji = emoji;
        }
    }

    public class MessageReactionRemoveAllEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the message affected.
        /// </summary>
        public Snowflake MessageId { get; }
        /// <summary>
        /// The ID of the channel the message is in.
        /// </summary>
        public Snowflake ChannelId { get; }

        internal MessageReactionRemoveAllEventArgs(Shard shard, Snowflake messageId, Snowflake channelId)
            : base(shard)
        {
            MessageId = messageId;
            ChannelId = channelId;
        }
    }

    public class VoiceStateUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The voice state of the user who's voice state changed.
        /// </summary>
        public DiscordVoiceState VoiceState { get; }

        internal VoiceStateUpdateEventArgs(Shard shard, DiscordVoiceState state) 
            : base(shard)
        {
            VoiceState = state;
        }
    }

    public class VoiceServerUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The voice server that changed.
        /// </summary>
        public DiscordVoiceServer VoiceServer { get; }

        internal VoiceServerUpdateEventArgs(Shard shard, DiscordVoiceServer server)
            : base(shard)
        {
            VoiceServer = server;
        }
    }
}
