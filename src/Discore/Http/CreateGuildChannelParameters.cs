using System;
using System.Collections.Generic;

namespace Discore.Http
{
    /// <summary>
    /// A set of parameters for creating a new text or voice guild channel.
    /// </summary>
    public class CreateGuildChannelParameters
    {
        /// <summary>
        /// Gets or sets the channel name (2-100 characters).
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets the type of guild channel.
        /// </summary>
        public DiscordGuildChannelType Type { get; }

        /// <summary>
        /// The voice bitrate (if a voice channel).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to set, if these parameters are not for a voice channel.
        /// </exception>
        public int? Bitrate
        {
            get => bitrate;
            set
            {
                if (Type != DiscordGuildChannelType.Voice)
                    throw new InvalidOperationException("Cannot set bitrate for non-voice channel.");

                bitrate = value;
            }
        }

        /// <summary>
        /// The user limit (if a voice channel).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to set, if these parameters are not for a voice channel.
        /// </exception>
        public int? UserLimit
        {
            get => userlimit;
            set
            {
                if (Type != DiscordGuildChannelType.Voice)
                    throw new InvalidOperationException("Cannot set user limit for non-voice channel.");

                userlimit = value;
            }
        }

        /// <summary>
        /// A list of permission overwrites.
        /// </summary>
        public IEnumerable<OverwriteParameters> PermissionOverwrites { get; set; }

        int? bitrate;
        int? userlimit;

        public CreateGuildChannelParameters(DiscordGuildChannelType type)
        {
            Type = type;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("name", Name);
            data.Set("type", Type.ToString().ToLower());
            
            if (Type == DiscordGuildChannelType.Voice)
            {
                data.Set("bitrate", bitrate);
                data.Set("userlimit", userlimit);
            }

            DiscordApiData permissionOverwritesArray = new DiscordApiData(DiscordApiDataType.Array);
            if (PermissionOverwrites != null)
            {
                foreach (OverwriteParameters overwriteParam in PermissionOverwrites)
                    permissionOverwritesArray.Values.Add(overwriteParam.Build());
            }

            data.Set("permission_overwrites", permissionOverwritesArray);

            return data;
        }
    }
}
