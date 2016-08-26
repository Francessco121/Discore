using Discore.Audio;
using Discore.Net;
using System;
using System.Collections.Generic;

namespace Discore
{
    public class DiscordGuildChannel : DiscordChannel
    {
        public DiscordGuild Guild { get; private set; }
        public string Name { get; private set; }
        public DiscordGuildChannelType GuildChannelType { get; private set; }
        public int Position { get; private set; }
        public bool IsPrivate { get; private set; }
        public Dictionary<string, DiscordOverwrite> RolePermissionOverwrites { get; private set; }
        public Dictionary<string, DiscordOverwrite> MemberPermissionOverwrites { get; private set; }
        public DiscordOverwrite[] AllPermissionOverwrites { get; private set; }
        public string Topic { get; private set; }
        public string LastMessageId { get; private set; }
        public int Bitrate { get; private set; }
        public int UserLimit { get; private set; }

        public DiscordGuildChannel(IDiscordClient client, DiscordGuild guild)
            : base(client, DiscordChannelType.Guild)
        {
            Guild = guild;
            RolePermissionOverwrites = new Dictionary<string, DiscordOverwrite>();
            MemberPermissionOverwrites = new Dictionary<string, DiscordOverwrite>();
        }

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
        public DiscordVoiceClient ConnectToVoice()
        {
            if (GuildChannelType != DiscordGuildChannelType.Voice)
                throw new InvalidOperationException("Attempted to voice connect to a non-voice channel");

            return Client.Gateway.ConnectToVoice(this);
        }

        public override string ToString()
        {
            return $"{GuildChannelType}:{Name}";
        }
    }
}
