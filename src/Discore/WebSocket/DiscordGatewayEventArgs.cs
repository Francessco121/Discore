using Discore.Voice;
using System;

namespace Discore.WebSocket
{
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

    public class PresenceEventArgs : GuildMemberEventArgs
    {
        /// <summary>
        /// Gets the presence state of the user.
        /// </summary>
        public DiscordUserPresence Presence { get; }

        internal PresenceEventArgs(Shard shard, Snowflake guildId, DiscordGuildMember member, DiscordUserPresence presence)
            : base(shard, guildId, member)
        {
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
        /// Gets the guild that the role is in.
        /// </summary>
        public DiscordGuild Guild { get; }
        /// <summary>
        /// Gets the role associated with the event.
        /// </summary>
        public DiscordRole Role { get; }

        internal GuildRoleEventArgs(Shard shard, DiscordGuild guild, DiscordRole role)
            : base(shard)
        {
            Guild = guild;
            Role = role;
        }
    }

    public class DMChannelEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the DM channel associated with the event.
        /// </summary>
        public DiscordDMChannel Channel { get; }

        internal DMChannelEventArgs(Shard shard, DiscordDMChannel channel)
            : base(shard)
        {
            Channel = channel;
        }
    }

    public class GuildEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the guild associated with the event.
        /// </summary>
        public DiscordGuild Guild { get; }

        internal GuildEventArgs(Shard shard, DiscordGuild guild)
            : base(shard)
        {
            Guild = guild;
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

    public class GuildChannelEventArgs : DiscordGatewayEventArgs
    {
        /// <summary>
        /// Gets the ID of the guild the channel is in.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// Gets the guild channel associated with the event.
        /// </summary>
        public DiscordGuildChannel Channel { get; }

        internal GuildChannelEventArgs(Shard shard, Snowflake guildId, DiscordGuildChannel channel)
            : base(shard)
        {
            GuildId = guildId;
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
        public DiscordMessage PartialMessage { get; }

        internal MessageUpdateEventArgs(Shard shard, DiscordMessage message)
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
