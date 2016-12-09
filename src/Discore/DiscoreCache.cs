namespace Discore
{
    public sealed class DiscoreCache
    {
        public DiscoreCacheTable<DiscoreGuildCache> Guilds { get; }
        public DiscoreCacheTable<DiscordUser> Users { get; }
        public DiscoreCacheTable<DiscordChannel> Channels { get; }
        public DiscoreCacheTable<DiscordDMChannel> DirectMessageChannels { get; }
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
    }

    public sealed class DiscoreGuildCache : DiscoreTypeCache<DiscordGuild>
    {
        public DiscoreCache Parent { get; }

        public DiscoreCacheTable<DiscordGuildChannel> Channels { get; }
        public DiscoreCacheTable<DiscordGuildTextChannel> TextChannels { get; }
        public DiscoreCacheTable<DiscordGuildVoiceChannel> VoiceChannels { get; }
        public DiscoreCacheTable<DiscordRole> Roles { get; }
        public DiscoreCacheTable<DiscordEmoji> Emojis { get; }
        public DiscoreCacheTable<DiscoreMemberCache> Members { get; }

        internal DiscoreGuildCache(DiscoreCache parent)
        {
            Parent = parent;

            Channels = new DiscoreCacheTable<DiscordGuildChannel>();
            TextChannels = new DiscoreCacheTable<DiscordGuildTextChannel>();
            VoiceChannels = new DiscoreCacheTable<DiscordGuildVoiceChannel>();
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
        internal DiscordGuildVoiceChannel SetChannel(DiscordGuildVoiceChannel voiceChannel)
        {
            VoiceChannels.Set(voiceChannel);
            Channels.Set(voiceChannel);
            Parent.Channels.Set(voiceChannel);

            return voiceChannel;
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
        internal DiscordGuildVoiceChannel RemoveVoiceChannel(Snowflake id)
        {
            DiscordGuildVoiceChannel channel = VoiceChannels.Remove(id);
            Channels.Remove(id);
            Parent.Channels.Remove(id);

            return channel;
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
        public DiscoreGuildCache Parent { get; }

        public DiscordUserPresence Presence { get; internal set; }
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
}
