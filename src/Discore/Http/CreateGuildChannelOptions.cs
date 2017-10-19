using System;
using System.Collections.Generic;

namespace Discore.Http
{
    /// <summary>
    /// A set of parameters for creating a new text or voice guild channel.
    /// </summary>
    public class CreateGuildChannelOptions
    {
        /// <summary>
        /// Gets or sets the channel name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets the type of guild channel.
        /// </summary>
        public DiscordChannelType Type { get; }

        /// <summary>
        /// Gets or sets the voice bitrate (if a voice channel).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this builder is not for a voice channel.</exception>
        public int? Bitrate
        {
            get => bitrate;
            set
            {
                if (Type != DiscordChannelType.GuildVoice)
                    throw new InvalidOperationException("Cannot set bitrate for non-voice channel.");

                bitrate = value;
            }
        }

        /// <summary>
        /// Gets or sets the user limit (if a voice channel).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this builder is not for a voice channel.</exception>
        public int? UserLimit
        {
            get => userlimit;
            set
            {
                if (Type != DiscordChannelType.GuildVoice)
                    throw new InvalidOperationException("Cannot set user limit for non-voice channel.");

                userlimit = value;
            }
        }

        /// <summary>
        /// Gets or sets the topic (if a text channel).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this builder is not for a text channel.</exception>
        public string Topic
        {
            get => topic;
            set
            {
                if (Type != DiscordChannelType.GuildText)
                    throw new InvalidOperationException("Cannot set topic for non-text channel.");

                topic = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this channel is NSFW (not-safe-for-work) (if a text channel).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this builder is not for a text channel.</exception>
        public bool? Nsfw
        {
            get => nsfw;
            set
            {
                if (Type != DiscordChannelType.GuildText)
                    throw new InvalidOperationException("Cannot set nsfw for non-text channel.");

                nsfw = value;
            }
        }

        /// <summary>
        /// Gets or sets a list of permission overwrites.
        /// </summary>
        public IList<OverwriteOptions> PermissionOverwrites { get; set; }

        int? bitrate;
        int? userlimit;
        string topic;
        bool? nsfw;

        /// <exception cref="ArgumentException">Thrown if <paramref name="type"/> is not a guild channel type.</exception>
        public CreateGuildChannelOptions(DiscordChannelType type)
        {
            if (type != DiscordChannelType.GuildText && type != DiscordChannelType.GuildVoice
                && type != DiscordChannelType.GuildCategory)
                throw new ArgumentException($"{type} is not a guild channel type!");

            Type = type;
        }

        /// <summary>
        /// Sets the channel name.
        /// </summary>
        public CreateGuildChannelOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the voice bitrate (if a voice channel).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this builder is not for a voice channel.</exception>
        public CreateGuildChannelOptions SetBitrate(int bitrate)
        {
            Bitrate = bitrate;
            return this;
        }

        /// <summary>
        /// Sets the user limit (if a voice channel).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this builder is not for a voice channel.</exception>
        public CreateGuildChannelOptions SetUserLimit(int userLimit)
        {
            UserLimit = userLimit;
            return this;
        }

        /// <summary>
        /// Sets the topic (if a text channel).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this builder is not for a text channel.</exception>
        public CreateGuildChannelOptions SetTopic(string topic)
        {
            Topic = topic;
            return this;
        }

        /// <summary>
        /// Sets whether this channel is NSFW (not-safe-for-work) (if a text channel).
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this builder is not for a text channel.</exception>
        public CreateGuildChannelOptions SetNsfw(bool nsfw)
        {
            Nsfw = nsfw;
            return this;
        }

        /// <summary>
        /// Adds a permission overwrite to the channel.
        /// </summary>
        public CreateGuildChannelOptions AddPermissionOverwrite(OverwriteOptions overwrite)
        {
            if (PermissionOverwrites == null)
                PermissionOverwrites = new List<OverwriteOptions>();

            PermissionOverwrites.Add(overwrite);
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("name", Name);
            data.Set("type", Type.ToString().ToLower());
            
            if (Type == DiscordChannelType.GuildVoice)
            {
                if (bitrate.HasValue)
                    data.Set("bitrate", bitrate.Value);
                if (userlimit.HasValue)
                    data.Set("userlimit", userlimit.Value);
            }
            else if (Type == DiscordChannelType.GuildText)
            {
                if (topic != null)
                    data.Set("topic", topic);
                if (nsfw.HasValue)
                    data.Set("nsfw", nsfw.Value);
            }

            if (PermissionOverwrites != null)
            {
                DiscordApiData permissionOverwritesArray = new DiscordApiData(DiscordApiDataType.Array);
                foreach (OverwriteOptions overwriteParam in PermissionOverwrites)
                    permissionOverwritesArray.Values.Add(overwriteParam.Build());

                data.Set("permission_overwrites", permissionOverwritesArray);
            }

            return data;
        }
    }
}
