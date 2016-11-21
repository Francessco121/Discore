using Discore.Http.Net;

namespace Discore.WebSocket
{
    /// <summary>
    /// A permission overwrite for a <see cref="DiscordRole"/> or <see cref="DiscordGuildMember"/>.
    /// </summary>
    public sealed class DiscordOverwrite : DiscordIdObject
    {
        /// <summary>
        /// The type of this overwrite.
        /// </summary>
        public DiscordOverwriteType Type { get; private set; }
        /// <summary>
        /// The specifically allowed permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Allow { get; private set; }
        /// <summary>
        /// The specifically denied permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Deny { get; private set; }

        public DiscordGuildChannel Channel { get; }

        HttpChannelsEndpoint channelsHttp;

        internal DiscordOverwrite(Shard shard, DiscordGuildChannel channel)
        {
            Channel = channel;
            channelsHttp = shard.Application.InternalHttpApi.Channels;
        }

        /// <summary>
        /// Edits the permissions of this overwrite.
        /// If successful, changes will be immediately reflected for this instance.
        /// </summary>
        /// <returns>Returns whether the operation was successful</returns>
        public bool Edit(DiscordPermission allow, DiscordPermission deny)
        {
            DiscordApiData data = channelsHttp.EditPermissions(Channel.Id, Id, allow, deny, Type);

            if (data.IsNull)
            {
                // These will be set by the gateway update sent after changing
                // the permissions, however we should provide immediate changes
                // when possible.

                Allow = allow;
                Deny = deny;
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// Deletes this overwrite.
        /// If successful, changes will be immediately reflected for the channel this overwrite was in.
        /// </summary>
        /// <returns>Returns whether the operation was successful</returns>
        public bool Delete()
        {
            DiscordApiData data = channelsHttp.DeletePermission(Channel.Id, Id);
            if (data.IsNull)
            {
                // If successful, reflect changes immediately.
                Channel.PermissionOverwrites.Remove(Id);
                Channel.RolePermissionOverwrites.Remove(Id);
                Channel.MemberPermissionOverwrites.Remove(Id);
                return true;
            }
            else
                return false;
        }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            string type = data.GetString("type");
            if (type != null)
            {
                switch (type)
                {
                    case "role":
                        Type = DiscordOverwriteType.Role;
                        break;
                    case "member":
                        Type = DiscordOverwriteType.Member;
                        break;
                }
            }

            long? allow = data.GetInt64("allow");
            if (allow.HasValue)
                Allow = (DiscordPermission)allow.Value;

            long? deny = data.GetInt64("deny");
            if (deny.HasValue)
                Deny = (DiscordPermission)deny.Value;
        }

        public override string ToString()
        {
            return $"{Type} Overwrite: {Id}";
        }
    }
}
