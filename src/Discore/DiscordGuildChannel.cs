using Discore.Audio;
using Discore.Net;
using System;
using System.Collections.Generic;

namespace Discore
{
    /// <summary>
    /// Guild channels represent an isolated set of users and messages within a <see cref="DiscordGuild"/>.
    /// </summary>
    public class DiscordGuildChannel : DiscordChannel
    {
        /// <summary>
        /// Gets the guild this channel is in.
        /// </summary>
        public DiscordGuild Guild { get; private set; }
        /// <summary>
        /// Gets the name of this channel.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the type of this guild channel.
        /// </summary>
        public DiscordGuildChannelType GuildChannelType { get; private set; }
        /// <summary>
        /// Gets the ordering position of this channel.
        /// </summary>
        public int Position { get; private set; }
        /// <summary>
        /// Gets whether or not this channel is private.
        /// </summary>
        public bool IsPrivate { get; private set; }
        /// <summary>
        /// All <see cref="DiscordOverwrite"/>s for the <see cref="DiscordRole"/>s in this channel.
        /// </summary>
        public Dictionary<string, DiscordOverwrite> RolePermissionOverwrites { get; private set; }
        /// <summary>
        /// All <see cref="DiscordOverwrite"/>s for the <see cref="DiscordGuildMember"/>s in this channel's guild.
        /// </summary>
        public Dictionary<string, DiscordOverwrite> MemberPermissionOverwrites { get; private set; }
        /// <summary>
        /// Gets every <see cref="DiscordOverwrite"/> specified by this channel.
        /// </summary>
        public DiscordOverwrite[] AllPermissionOverwrites { get; private set; }
        /// <summary>
        /// Gets the topic of this channel.
        /// </summary>
        public string Topic { get; private set; }
        /// <summary>
        /// Gets the id of the last message sent in this channel (if a text channel).
        /// </summary>
        public string LastMessageId { get; private set; }
        /// <summary>
        /// Gets the bitrate of this channel (if a voice channel).
        /// </summary>
        public int Bitrate { get; private set; }
        /// <summary>
        /// Gets the user limit of this channel (if a voice channel).
        /// </summary>
        public int UserLimit { get; private set; }

        /// <summary>
        /// Creates a new <see cref="DiscordGuildChannel"/> instance.
        /// </summary>
        /// <param name="client">The <see cref="IDiscordClient"/> associated with this channel.</param>
        /// <param name="guild">The <see cref="DiscordGuild"/> this channel is in.</param>
        public DiscordGuildChannel(IDiscordClient client, DiscordGuild guild)
            : base(client, DiscordChannelType.Guild)
        {
            Guild = guild;
            RolePermissionOverwrites = new Dictionary<string, DiscordOverwrite>();
            MemberPermissionOverwrites = new Dictionary<string, DiscordOverwrite>();
        }

        /// <summary>
        /// Changes the properties of this channel.
        /// </summary>
        /// <param name="modifyParams">The changed properties.</param>
        /// <exception cref="ArgumentException">Thrown if the params are for a different type of guild channel.</exception>
        public void Modify(DiscordGuildChannelModifyParams modifyParams)
        {
            if (modifyParams.Type != GuildChannelType)
                throw new ArgumentException("Channel modify params must be for the same type of guild channel");

            Name = modifyParams.Name;
            Position = modifyParams.Position;

            if (modifyParams.Type == DiscordGuildChannelType.Text)
            {
                Topic = modifyParams.Topic;
            }
            else if (modifyParams.Type == DiscordGuildChannelType.Voice)
            {
                Bitrate = modifyParams.Bitrate;
                UserLimit = modifyParams.UserLimit;
            }

            Client.Rest.Channels.Modify(this, modifyParams);
        }

        /// <summary>
        /// Updates this channel with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this channel with.</param>
        public override void Update(DiscordApiData data)
        {
            Id            = data.GetString("id") ?? Id;
            Name          = data.GetString("name") ?? Name;
            Position      = data.GetInteger("position") ?? Position;
            IsPrivate     = data.GetBoolean("is_private") ?? IsPrivate;
            Topic         = data.GetString("topic") ?? Topic;
            LastMessageId = data.GetString("last_message_id") ?? LastMessageId;
            Bitrate       = data.GetInteger("bitrate") ?? Bitrate;
            UserLimit     = data.GetInteger("user_limit") ?? UserLimit;

            IReadOnlyList<DiscordApiData> permissionOverwrites = data.GetArray("permission_overwrites");
            if (permissionOverwrites != null)
            {
                RolePermissionOverwrites.Clear();
                MemberPermissionOverwrites.Clear();
                AllPermissionOverwrites = new DiscordOverwrite[permissionOverwrites.Count];

                for (int i = 0; i < permissionOverwrites.Count; i++)
                {
                    DiscordOverwrite overwrite = new DiscordOverwrite();
                    overwrite.Update(permissionOverwrites[i]);

                    if (overwrite.Type == DiscordOverwriteType.Member)
                        MemberPermissionOverwrites.Add(overwrite.Id, overwrite);
                    else if (overwrite.Type == DiscordOverwriteType.Role)
                        RolePermissionOverwrites.Add(overwrite.Id, overwrite);

                    AllPermissionOverwrites[i] = overwrite;
                }
            }

            string type = data.GetString("type");
            if (type != null)
                GuildChannelType = type == "text" ? DiscordGuildChannelType.Text : DiscordGuildChannelType.Voice;
        }

        /// <summary>
        /// Attempts to connect to this voice channel. This is a non-blocking call.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if this channel is not a voice channel.</exception>
        public DiscordVoiceClient ConnectToVoice()
        {
            if (GuildChannelType != DiscordGuildChannelType.Voice)
                throw new InvalidOperationException("Attempted to voice connect to a non-voice channel");

            return Client.Gateway.ConnectToVoice(this);
        }

        /// <summary>
        /// Gets a string representation of this channel.
        /// </summary>
        /// <returns>Returns the string representation of this channel.</returns>
        public override string ToString()
        {
            return $"{GuildChannelType}:{Name}";
        }
    }
}
