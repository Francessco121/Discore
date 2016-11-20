using Discore.Http.Net;
using System.Collections.Generic;

namespace Discore.WebSocket
{
    public abstract class DiscordGuildChannel : DiscordChannel
    {
        public DiscordGuildChannelType GuildChannelType { get; }

        /// <summary>
        /// Gets the name of this channel.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the UI ordering position of this channel.
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// Gets a table of all permission overwrites associated with this channel.
        /// </summary>
        public DiscordApiCacheTable<DiscordOverwrite> PermissionOverwrites { get; }

        /// <summary>
        /// Gets a list of all role permission overwrites associated with this channel.
        /// </summary>
        public DiscordApiCacheIdSet<DiscordOverwrite> RolePermissionOverwrites { get; }

        /// <summary>
        /// Gets a list of all member permission overwrites associated with this channel.
        /// </summary>
        public DiscordApiCacheIdSet<DiscordOverwrite> MemberPermissionOverwrites { get; }

        /// <summary>
        /// Gets the guild this channel is in.
        /// </summary>
        public DiscordGuild Guild { get; }

        Shard shard;
        HttpChannelsEndpoint channelsHttp;

        internal DiscordGuildChannel(Shard shard, DiscordGuild guild, DiscordGuildChannelType type) 
            : base(shard, DiscordChannelType.Guild)
        {
            this.shard = shard;
            Guild = guild;
            GuildChannelType = type;

            channelsHttp = shard.Application.InternalHttpApi.Channels;

            PermissionOverwrites = new DiscordApiCacheTable<DiscordOverwrite>();
            RolePermissionOverwrites = new DiscordApiCacheIdSet<DiscordOverwrite>(PermissionOverwrites);
            MemberPermissionOverwrites = new DiscordApiCacheIdSet<DiscordOverwrite>(PermissionOverwrites);
        }

        public bool EditPermissions(DiscordGuildMember member, DiscordPermission allow, DiscordPermission deny)
        {
            return EditPermissions(member.Id, allow, deny, DiscordOverwriteType.Member);
        }

        public bool EditPermissions(DiscordRole role, DiscordPermission allow, DiscordPermission deny)
        {
            return EditPermissions(role.Id, allow, deny, DiscordOverwriteType.Role);
        }

        bool EditPermissions(Snowflake overwriteId, DiscordPermission allow, DiscordPermission deny,
            DiscordOverwriteType type)
        {
            DiscordApiData data = channelsHttp.EditPermissions(Id, overwriteId, allow, deny, type);
            return data.IsNull;
        }

        public bool DeletePermission(DiscordGuildMember member)
        {
            return DeletePermission(member.Id);
        }

        public bool DeletePermission(DiscordRole role)
        {
            return DeletePermission(role.Id);
        }

        public bool DeletePermission(DiscordOverwrite overwrite)
        {
            return DeletePermission(overwrite.Id);
        }

        bool DeletePermission(Snowflake overwriteId)
        {
            DiscordApiData data = channelsHttp.DeletePermission(Id, overwriteId);
            return data.IsNull;
        }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Name = data.GetString("name") ?? Name;
            Position = data.GetInteger("position") ?? Position;

            IList<DiscordApiData> overwrites = data.GetArray("permission_overwrites");
            if (overwrites != null)
            {
                PermissionOverwrites.Clear();
                foreach (DiscordApiData overwriteData in overwrites)
                {
                    Snowflake id = overwriteData.GetSnowflake("id").Value;
                    DiscordOverwrite overwrite = PermissionOverwrites.Edit(id, 
                        () => new DiscordOverwrite(), o => o.Update(overwriteData));

                    if (overwrite.Type == DiscordOverwriteType.Member)
                        MemberPermissionOverwrites.Add(id);
                    else if (overwrite.Type == DiscordOverwriteType.Role)
                        RolePermissionOverwrites.Add(id);
                }
            }
        }

        public override string ToString()
        {
            return $"{GuildChannelType} Channel: {Name}";
        }
    }
}
