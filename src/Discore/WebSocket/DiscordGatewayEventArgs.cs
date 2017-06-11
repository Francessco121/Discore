using Discore.Voice;
using System;

namespace Discore.WebSocket
{
    public abstract class DiscordGatewayEventArgs : EventArgs
    {
        public Shard Shard { get; }

        public DiscordGatewayEventArgs(Shard shard)
        {
            Shard = shard;
        }
    }

    public class IntegrationEventArgs : DiscordGatewayEventArgs
    {
        public DiscordIntegration Integration { get; }

        public IntegrationEventArgs(Shard shard, DiscordIntegration integration)
            : base(shard)
        {
            Integration = integration;
        }
    }

    public class UserEventArgs : DiscordGatewayEventArgs
    {
        public DiscordUser User { get; }

        public UserEventArgs(Shard shard, DiscordUser user)
            : base(shard)
        {
            User = user;
        }
    }

    public class TypingStartEventArgs : DiscordGatewayEventArgs
    {
        public Snowflake UserId { get; }
        public Snowflake ChannelId { get; }
        /// <summary>
        /// Unix time in seconds when the typing started.
        /// </summary>
        public int Timestamp { get; }

        public TypingStartEventArgs(Shard shard, Snowflake userId, Snowflake channelId, int timestamp)
            : base(shard)
        {
            UserId = userId;
            ChannelId = channelId;
            Timestamp = timestamp;
        }
    }

    public class GuildMemberEventArgs : DiscordGatewayEventArgs
    {
        public Snowflake GuildId { get; }
        public DiscordGuildMember Member { get; }

        public GuildMemberEventArgs(Shard shard, Snowflake guildId, DiscordGuildMember member)
            : base(shard)
        {
            GuildId = guildId;
            Member = member;
        }
    }

    public class PresenceEventArgs : GuildMemberEventArgs
    {
        public DiscordUserPresence Presence { get; }

        public PresenceEventArgs(Shard shard, Snowflake guildId, DiscordGuildMember member, DiscordUserPresence presence)
            : base(shard, guildId, member)
        {
            Presence = presence;
        }
    }

    public class GuildMemberChunkEventArgs : DiscordGatewayEventArgs
    {
        public Snowflake GuildId { get; }
        public DiscordGuildMember[] Members { get; }

        public GuildMemberChunkEventArgs(Shard shard, Snowflake guildId, DiscordGuildMember[] members)
            : base(shard)
        {
            GuildId = guildId;
            Members = members;
        }
    }

    public class GuildUserEventArgs : DiscordGatewayEventArgs
    {
        public Snowflake GuildId { get; }
        public DiscordUser User { get; }

        public GuildUserEventArgs(Shard shard, Snowflake guildId, DiscordUser user)
            : base(shard)
        {
            GuildId = guildId;
            User = user;
        }
    }

    public class GuildRoleEventArgs : DiscordGatewayEventArgs
    {
        public DiscordGuild Guild { get; }
        public DiscordRole Role { get; }

        public GuildRoleEventArgs(Shard shard, DiscordGuild guild, DiscordRole role)
            : base(shard)
        {
            Guild = guild;
            Role = role;
        }
    }

    public class DMChannelEventArgs : DiscordGatewayEventArgs
    {
        public DiscordDMChannel Channel { get; }

        public DMChannelEventArgs(Shard shard, DiscordDMChannel channel)
            : base(shard)
        {
            Channel = channel;
        }
    }

    public class GuildEventArgs : DiscordGatewayEventArgs
    {
        public DiscordGuild Guild { get; }

        public GuildEventArgs(Shard shard, DiscordGuild guild)
            : base(shard)
        {
            Guild = guild;
        }
    }

    public class GuildIntegrationsEventArgs : DiscordGatewayEventArgs
    {
        public Snowflake GuildId { get; }

        public GuildIntegrationsEventArgs(Shard shard, Snowflake guildId)
            : base(shard)
        {
            GuildId = guildId;
        }
    }

    public class GuildChannelEventArgs : DiscordGatewayEventArgs
    {
        public Snowflake GuildId { get; }
        public DiscordGuildChannel Channel { get; }

        public GuildChannelEventArgs(Shard shard, Snowflake guildId, DiscordGuildChannel channel)
            : base(shard)
        {
            GuildId = guildId;
            Channel = channel;
        }
    }

    public class WebhooksUpdateEventArgs : DiscordGatewayEventArgs
    {
        public Snowflake GuildId { get; }
        public Snowflake ChannelId { get; }

        public WebhooksUpdateEventArgs(Shard shard, Snowflake guildId, Snowflake channelId)
            : base(shard)
        {
            GuildId = guildId;
            ChannelId = channelId;
        }
    }

    public class ChannelPinsUpdateEventArgs : DiscordGatewayEventArgs
    {
        public Snowflake ChannelId { get; }
        /// <summary>
        /// Gets the date-time of the newest pin as of this update (or null if there is no longer any pins).
        /// </summary>
        public DateTime? LastPinTimestamp { get; }

        public ChannelPinsUpdateEventArgs(Shard shard, Snowflake channelId, DateTime? lastPinTimestamp)
            : base(shard)
        {
            ChannelId = channelId;
            LastPinTimestamp = lastPinTimestamp;
        }
    }

    public class MessageEventArgs : DiscordGatewayEventArgs
    {
        public DiscordMessage Message { get; }

        public MessageEventArgs(Shard shard, DiscordMessage message)
            : base(shard)
        {
            Message = message;
        }
    }

    public class MessageUpdateEventArgs : DiscordGatewayEventArgs
    {
        public DiscordMessage PartialMessage { get; }

        public MessageUpdateEventArgs(Shard shard, DiscordMessage message)
            : base(shard)
        {
            PartialMessage = message;
        }
    }

    public class MessageDeleteEventArgs : DiscordGatewayEventArgs
    {
        public Snowflake MessageId { get; }
        public Snowflake ChannelId { get; }

        public MessageDeleteEventArgs(Shard shard, Snowflake messageId, Snowflake channelId)
            : base(shard)
        {
            MessageId = messageId;
            ChannelId = channelId;
        }
    }

    public class MessageReactionEventArgs : DiscordGatewayEventArgs
    {
        public Snowflake MessageId { get; }
        public Snowflake UserId { get; }
        public Snowflake ChannelId { get; }
        public DiscordReactionEmoji Emoji { get; }

        public MessageReactionEventArgs(Shard shard, Snowflake messageId, Snowflake channelId,
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
        public Snowflake MessageId { get; }
        public Snowflake ChannelId { get; }

        public MessageReactionRemoveAllEventArgs(Shard shard, Snowflake messageId, Snowflake channelId)
            : base(shard)
        {
            MessageId = messageId;
            ChannelId = channelId;
        }
    }

    public class ShardExceptionEventArgs : DiscordGatewayEventArgs
    {
        public Exception Exception { get; }

        public ShardExceptionEventArgs(Shard shard, Exception exception)
            : base(shard)
        {
            Exception = exception;
        }
    }

    public class VoiceStateEventArgs : DiscordGatewayEventArgs
    {
        public DiscordVoiceState VoiceState { get; }

        public VoiceStateEventArgs(Shard shard, DiscordVoiceState state) 
            : base(shard)
        {
            VoiceState = state;
        }
    }
}
