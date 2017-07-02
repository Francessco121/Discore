namespace Discore.Http
{
    /// <summary>
    /// An optional set of parameters for modifying a guild voice channel.
    /// </summary>
    public class GuildVoiceChannelOptions
    {
        /// <summary>
        /// Gets or sets the name of the channel (or null to leave unchanged).
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the sorting position of the channel (or null to leave unchanged).
        /// </summary>
        public int? Position { get; set; }

        /// <summary>
        /// Gets or sets the bitrate of the voice channel (or null to leave unchanged). 
        /// </summary>
        public int? Bitrate { get; set; }

        /// <summary>
        /// Gets or sets the user limit of the voice channel (or null to leave unchanged).
        /// <para>Set to zero to remove the user limit.</para>
        /// </summary>
        public int? UserLimit { get; set; }

        /// <summary>
        /// Sets the name of the channel.
        /// </summary>
        public GuildVoiceChannelOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the sorting position of the channel.
        /// </summary>
        public GuildVoiceChannelOptions SetPosition(int position)
        {
            Position = position;
            return this;
        }

        /// <summary>
        /// Sets the bitrate of the voice channel.
        /// </summary>
        public GuildVoiceChannelOptions SetBitrate(int bitrate)
        {
            Bitrate = bitrate;
            return this;
        }

        /// <summary>
        /// Sets the user limit of the voice channel.
        /// </summary>
        /// <param name="userLimit">The maximum number of users or zero to remove the limit.</param>
        public GuildVoiceChannelOptions SetUserLimit(int userLimit)
        {
            UserLimit = userLimit;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);

            if (Name != null)
                data.Set("name", Name);
            if (Position.HasValue)
                data.Set("position", Position.Value);
            if (Bitrate.HasValue)
                data.Set("bitrate", Bitrate.Value);
            if (UserLimit.HasValue)
                data.Set("user_limit", UserLimit.Value);

            return data;
        }
    }
}
