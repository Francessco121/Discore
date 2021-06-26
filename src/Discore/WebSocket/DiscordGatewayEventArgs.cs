using Discore.Voice;
using System;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    // TODO: audit event names

    public abstract class DiscordGatewayEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the shard of the Gateway connection that fired the event.
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
            /// Gets the shard ID that is associated with the Gateway connection.
            /// </summary>
            public int Id { get; }

            /// <summary>
            /// Gets the total number of shards provided to the Gateway connection.
            /// </summary>
            public int Total { get; }

            internal ShardInfo(int id, int total)
            {
                Id = id;
                Total = total;
            }
        }

        /// <summary>
        /// Gets the user of the bot application.
        /// </summary>
        public DiscordUser User { get; }

        /// <summary>
        /// Gets the IDs of every guild that the bot is in.
        /// </summary>
        public Snowflake[] GuildIds { get; }

        /// <summary>
        /// Gets information about the shard.
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

    public class UserEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the user associated with the event.
        /// </summary>
        public DiscordUser User { get; }

        internal UserEventArgs(Shard shard, DiscordUser user)
            : base(shard)
        {
            User = user;
        }
    }

    public class TypingStartEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the ID of the user that started typing.
        /// </summary>
        public Snowflake UserId { get; }
        /// <summary>
        /// Gets the ID of the text channel that the user starting typing in.
        /// </summary>
        public Snowflake ChannelId { get; }
        /// <summary>
        /// Unix time in seconds when the typing started.
        /// </summary>
        public int Timestamp { get; }

        internal TypingStartEventArgs(Shard shard, Snowflake userId, Snowflake channelId, int timestamp)
            : base(shard)
        {
            UserId = userId;
            ChannelId = channelId;
            Timestamp = timestamp;
        }
    }

    public class GuildMemberEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the ID of the guild the member is in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// Gets the member associated with the event.
        /// </summary>
        public DiscordGuildMember Member { get; }

        internal GuildMemberEventArgs(Shard shard, Snowflake guildId, DiscordGuildMember member)
            : base(shard)
        {
            GuildId = guildId;
            Member = member;
        }
    }

    public class GuildMemberUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the ID of the guild the member is in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// Gets partial updated data for the member.
        /// </summary>
        public DiscordPartialGuildMember PartialMember { get; }

        internal GuildMemberUpdateEventArgs(Shard shard, Snowflake guildId, DiscordPartialGuildMember partialMember)
            : base(shard)
        {
            GuildId = guildId;
            PartialMember = partialMember;
        }
    }

    public class PresenceEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the ID of the guild the member is in.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// Gets the presence state of the user.
        /// </summary>
        public DiscordUserPresence Presence { get; }

        internal PresenceEventArgs(Shard shard, Snowflake guildId, DiscordUserPresence presence)
            : base(shard)
        {
            GuildId = guildId;
            Presence = presence;
        }
    }

    public class GuildMemberChunkEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the ID of the guild that the members are in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// Gets a list of all members included in the chunk.
        /// </summary>
        public DiscordGuildMember[] Members { get; }

        internal GuildMemberChunkEventArgs(Shard shard, Snowflake guildId, DiscordGuildMember[] members)
            : base(shard)
        {
            GuildId = guildId;
            Members = members;
        }
    }

    public class GuildUserEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the ID of the guild associated with the event.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// Gets the user associated with the event.
        /// </summary>
        public DiscordUser User { get; }

        internal GuildUserEventArgs(Shard shard, Snowflake guildId, DiscordUser user)
            : base(shard)
        {
            GuildId = guildId;
            User = user;
        }
    }

    public class GuildRoleEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild that the role is in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// Gets the role associated with the event.
        /// </summary>
        public DiscordRole Role { get; }

        internal GuildRoleEventArgs(Shard shard, Snowflake guildId, DiscordRole role)
            : base(shard)
        {
            GuildId = guildId;
            Role = role;
        }
    }

    public class GuildRoleIdEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild that the role is in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// The ID of the role associated with the event.
        /// </summary>
        public Snowflake RoleId { get; }

        internal GuildRoleIdEventArgs(Shard shard, Snowflake guildId, Snowflake roleId)
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
        /// Gets the guild associated with the event.
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
        /// Gets the guild associated with the event.
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

    public class GuildEmojisEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// The ID of the guild.
        /// </summary>
        public Snowflake GuildId { get; }

        /// <summary>
        /// The new list of guild emojis.
        /// </summary>
        public IReadOnlyList<DiscordEmoji> Emojis { get; }

        internal GuildEmojisEventArgs(Shard shard, Snowflake guildId, IReadOnlyList<DiscordEmoji> emojis)
            : base(shard)
        {
            GuildId = guildId;
            Emojis = emojis;
        }
    }

    public class GuildIntegrationsEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the ID of the guild that had its integrations updated.
        /// </summary>
        public Snowflake GuildId { get; }

        internal GuildIntegrationsEventArgs(Shard shard, Snowflake guildId)
            : base(shard)
        {
            GuildId = guildId;
        }
    }

    public class ChannelEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the channel associated with the event.
        /// </summary>
        public DiscordChannel Channel { get; }

        internal ChannelEventArgs(Shard shard, DiscordChannel channel)
            : base(shard)
        {
            Channel = channel;
        }
    }

    public class WebhooksUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the ID of the guild that had a webhook updated.
        /// <para>This is also the ID of the guild that the webhook's channel is in.</para>
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// Gets the ID of the channel that had a webhook updated.
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
        /// Gets the ID of the text channel that had its pins updated.
        /// </summary>
        public Snowflake ChannelId { get; }
        /// <summary>
        /// Gets the date-time of the newest pin as of this update (or null if there is no longer any pins).
        /// </summary>
        public DateTime? LastPinTimestamp { get; }

        internal ChannelPinsUpdateEventArgs(Shard shard, Snowflake channelId, DateTime? lastPinTimestamp)
            : base(shard)
        {
            ChannelId = channelId;
            LastPinTimestamp = lastPinTimestamp;
        }
    }

    public class MessageEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the message associated with the event.
        /// </summary>
        public DiscordMessage Message { get; }

        internal MessageEventArgs(Shard shard, DiscordMessage message)
            : base(shard)
        {
            Message = message;
        }
    }

    public class MessageUpdateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets a partial message object representing the changes made to the message.
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
        /// Gets the ID of the message that was deleted.
        /// </summary>
        public Snowflake MessageId { get; }
        /// <summary>
        /// Gets the ID of the channel the message was deleted from.
        /// </summary>
        public Snowflake ChannelId { get; }

        internal MessageDeleteEventArgs(Shard shard, Snowflake messageId, Snowflake channelId)
            : base(shard)
        {
            MessageId = messageId;
            ChannelId = channelId;
        }
    }

    public class MessageReactionEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the ID of the message associated with the reaction.
        /// </summary>
        public Snowflake MessageId { get; }
        /// <summary>
        /// Gets the ID of the user who added/removed the reaction.
        /// </summary>
        public Snowflake UserId { get; }
        /// <summary>
        /// Gets the ID of the channel the message affected is in.
        /// </summary>
        public Snowflake ChannelId { get; }
        /// <summary>
        /// Gets the emoji associated with the event.
        /// </summary>
        public DiscordReactionEmoji Emoji { get; }

        internal MessageReactionEventArgs(Shard shard, Snowflake messageId, Snowflake channelId,
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
        /// Gets the ID of the message affected.
        /// </summary>
        public Snowflake MessageId { get; }
        /// <summary>
        /// Gets the ID of the channel the message is in.
        /// </summary>
        public Snowflake ChannelId { get; }

        internal MessageReactionRemoveAllEventArgs(Shard shard, Snowflake messageId, Snowflake channelId)
            : base(shard)
        {
            MessageId = messageId;
            ChannelId = channelId;
        }
    }

    public class VoiceStateEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the voice state of the user who's voice state changed.
        /// </summary>
        public DiscordVoiceState VoiceState { get; }

        internal VoiceStateEventArgs(Shard shard, DiscordVoiceState state) 
            : base(shard)
        {
            VoiceState = state;
        }
    }
}
