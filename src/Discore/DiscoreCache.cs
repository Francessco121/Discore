using Discore.Voice;

namespace Discore
{
    public sealed class DiscoreCache
    {
        /// <summary>
        /// Gets a table of all cached guilds.
        /// </summary>
        public DiscoreCacheTable<DiscoreGuildCache> Guilds { get; }
        /// <summary>
        /// Gets a table of all cached users.
        /// </summary>
        public DiscoreCacheTable<DiscordUser> Users { get; }
        /// <summary>
        /// Gets a table of all cached channels.
        /// </summary>
        public DiscoreCacheTable<DiscordChannel> Channels { get; }
        /// <summary>
        /// Gets a table of all cached DM channels.
        /// </summary>
        public DiscoreCacheTable<DiscordDMChannel> DirectMessageChannels { get; }
        /// <summary>
        /// Gets a table of all cached roles.
        /// </summary>
        public DiscoreCacheTable<DiscordRole> Roles { get; }

        internal DiscoreCache()
        {
            Guilds = new DiscoreCacheTable<DiscoreGuildCache>();
            Users = new DiscoreCacheTable<DiscordUser>();
            Channels = new DiscoreCacheTable<DiscordChannel>();
            DirectMessageChannels = new DiscoreCacheTable<DiscordDMChannel>();
            Roles = new DiscoreCacheTable<DiscordRole>();
        }

        /// <summary>
        /// Will update the cache with the specified dm channel and handle aliases.
        /// </summary>
        internal DiscordDMChannel SetDMChannel(DiscordDMChannel dm)
        {
            DirectMessageChannels.Set(dm);
            Channels.Set(dm);

            return dm;
        }

        /// <summary>
        /// Will remove the channel from the cache and all aliases.
        /// </summary>
        internal DiscordDMChannel RemoveDMChannel(Snowflake id)
        {
            DiscordDMChannel dm = DirectMessageChannels.Remove(id);
            Channels.Remove(id);

            return dm;
        }

        internal void Clear()
        {
            Guilds.Clear();
            Users.Clear();
            Channels.Clear();
            DirectMessageChannels.Clear();
            Roles.Clear();
        }
    }

    public abstract class DiscoreTypeCache<T> : DiscordHashableObject
        where T : DiscordHashableObject
    {
        /// <summary>
        /// Gets the current state of the item this cache represents.
        /// </summary>
        public T Value { get; internal set; }

        internal override Snowflake DictionaryId { get { return Value.DictionaryId; } }

        internal DiscoreTypeCache() { }

        internal abstract void Clear();

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public sealed class DiscoreGuildCache : DiscoreTypeCache<DiscordGuild>
    {
        /// <summary>
        /// Gets the parent cache.
        /// </summary>
        public DiscoreCache Parent { get; }

        /// <summary>
        /// Gets a table of all cached channels in this guild.
        /// </summary>
        public DiscoreCacheTable<DiscordGuildChannel> Channels { get; }
        /// <summary>
        /// Gets a table of all cached text channels in this guild.
        /// </summary>
        public DiscoreCacheTable<DiscordGuildTextChannel> TextChannels { get; }
        /// <summary>
        /// Gets a table of all cached voice channels in this guild.
        /// </summary>
        public DiscoreCacheTable<DiscoreVoiceChannelCache> VoiceChannels { get; }
        /// <summary>
        /// Gets a table of all cached roles in this guild.
        /// </summary>
        public DiscoreCacheTable<DiscordRole> Roles { get; }
        /// <summary>
        /// Gets a table of all cached emojis in this guild.
        /// </summary>
        public DiscoreCacheTable<DiscordEmoji> Emojis { get; }
        /// <summary>
        /// Gets a table of all cached members in this guild.
        /// </summary>
        public DiscoreCacheTable<DiscoreMemberCache> Members { get; }

        internal DiscoreGuildCache(DiscoreCache parent)
        {
            Parent = parent;

            Channels = new DiscoreCacheTable<DiscordGuildChannel>();
            TextChannels = new DiscoreCacheTable<DiscordGuildTextChannel>();
            VoiceChannels = new DiscoreCacheTable<DiscoreVoiceChannelCache>();
            Roles = new DiscoreCacheTable<DiscordRole>();
            Emojis = new DiscoreCacheTable<DiscordEmoji>();
            Members = new DiscoreCacheTable<DiscoreMemberCache>();
        }

        /// <summary>
        /// Will update the cache with the specified role and handle aliases.
        /// </summary>
        internal DiscordRole SetRole(DiscordRole role)
        {
            Roles.Set(role);
            Parent.Roles.Set(role);

            return role;
        }

        /// <summary>
        /// Will remove the role from the cache and all aliases.
        /// </summary>
        internal DiscordRole RemoveRole(Snowflake id)
        {
            DiscordRole role = Roles.Remove(id);
            Parent.Roles.Remove(id);

            return role;
        }

        /// <summary>
        /// Will update the cache with the specified text channel and handle aliases.
        /// </summary>
        internal DiscordGuildTextChannel SetChannel(DiscordGuildTextChannel textChannel)
        {
            TextChannels.Set(textChannel);
            Channels.Set(textChannel);
            Parent.Channels.Set(textChannel);

            return textChannel;
        }

        /// <summary>
        /// Will update the cache with the specified voice channel and handle aliases.
        /// </summary>
        internal DiscoreVoiceChannelCache SetChannel(DiscordGuildVoiceChannel voiceChannel)
        {
            DiscoreVoiceChannelCache voiceChannelCache = VoiceChannels.Get(voiceChannel.Id);
            if (voiceChannelCache == null)
            {
                voiceChannelCache = new DiscoreVoiceChannelCache(this);
                voiceChannelCache.Value = voiceChannel;
                VoiceChannels.Set(voiceChannelCache);
            }
            else
                voiceChannelCache.Value = voiceChannel;

            Channels.Set(voiceChannel);
            Parent.Channels.Set(voiceChannel);

            return voiceChannelCache;
        }

        /// <summary>
        /// Will remove the text channel from the cache and all aliases.
        /// </summary>
        internal DiscordGuildTextChannel RemoveTextChannel(Snowflake id)
        {
            DiscordGuildTextChannel channel = TextChannels.Remove(id);
            Channels.Remove(id);
            Parent.Channels.Remove(id);

            return channel;
        }

        /// <summary>
        /// Will remove the voice channel from the cache and all aliases.
        /// </summary>
        internal DiscoreVoiceChannelCache RemoveVoiceChannel(Snowflake id)
        {
            DiscoreVoiceChannelCache channelCache = VoiceChannels.Remove(id);
            Channels.Remove(id);
            Parent.Channels.Remove(id);

            return channelCache;
        }

        internal override void Clear()
        {
            Channels.Clear();
            TextChannels.Clear();
            VoiceChannels.Clear();
            Roles.Clear();
            Emojis.Clear();
            Members.Clear();
        }
    }

    public sealed class DiscoreMemberCache : DiscoreTypeCache<DiscordGuildMember>
    {
        /// <summary>
        /// Gets the cache of the guild this member is in.
        /// </summary>
        public DiscoreGuildCache Parent { get; }

        /// <summary>
        /// Gets the current presence state of this member (or null if not available).
        /// </summary>
        public DiscordUserPresence Presence { get; internal set; }
        /// <summary>
        /// Gets the current voice state of this member (or null if not available).
        /// </summary>
        public DiscordVoiceState VoiceState { get; internal set; }

        internal DiscoreMemberCache(DiscoreGuildCache parent)
        {
            Parent = parent;
        }

        internal override void Clear()
        {
            Presence = null;
            VoiceState = null;
        }
    }

    public sealed class DiscoreVoiceChannelCache : DiscoreTypeCache<DiscordGuildVoiceChannel>
    {
        /// <summary>
        /// Gets the cache of the guild this voice channels is in.
        /// </summary>
        public DiscoreGuildCache Parent { get; }

        /// <summary>
        /// Gets a table of all members currently connected to this voice channel.
        /// </summary>
        public DiscoreCacheTable<DiscoreMemberCache> ConnectedMembers { get; }

        internal DiscoreVoiceChannelCache(DiscoreGuildCache parent)
        {
            Parent = parent;

            ConnectedMembers = new DiscoreCacheTable<DiscoreMemberCache>();
        }

        internal override void Clear()
        {
            ConnectedMembers.Clear();
        }
    }
}
