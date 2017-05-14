using Discore.Http;
using System;
using System.Threading.Tasks;

namespace Discore
{
    /// <summary>
    /// A permission overwrite for a <see cref="DiscordRole"/> or <see cref="DiscordGuildMember"/>.
    /// </summary>
    public sealed class DiscordOverwrite : DiscordIdObject
    {
        public Snowflake ChannelId { get; }

        /// <summary>
        /// The type of this overwrite.
        /// </summary>
        public DiscordOverwriteType Type { get; }
        /// <summary>
        /// The specifically allowed permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Allow { get; }
        /// <summary>
        /// The specifically denied permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Deny { get; }

        DiscordHttpApi http;

        internal DiscordOverwrite(IDiscordApplication app, Snowflake channelId, DiscordApiData data)
            : base(data)
        {
            http = app.HttpApi;

            ChannelId = channelId;

            string typeStr = data.GetString("type");
            DiscordOverwriteType type;
            if (Enum.TryParse(typeStr, true, out type))
                Type = type;

            long allow = data.GetInt64("allow").Value;
            Allow = (DiscordPermission)allow;

            long deny = data.GetInt64("deny").Value;
            Deny = (DiscordPermission)deny;
        }

        /// <summary>
        /// Edits the permissions of this overwrite.
        /// If successful, changes will be immediately reflected for this instance.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task Edit(DiscordPermission allow, DiscordPermission deny)
        {
            return http.EditChannelPermissions(ChannelId, Id, allow, deny, Type);
        }

        /// <summary>
        /// Deletes this overwrite.
        /// If successful, changes will be immediately reflected for the channel this overwrite was in.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task Delete()
        {
            return http.DeleteChannelPermission(ChannelId, Id);
        }

        public override string ToString()
        {
            return $"{Type} Overwrite: {Id}";
        }
    }
}
