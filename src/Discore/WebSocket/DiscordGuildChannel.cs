using Discore.Http.Net;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        /// <summary>
        /// Adds/edits a guild member permission overwrite for this channel.
        /// </summary>
        /// <param name="member">The member this overwrite will change permissions for.</param>
        /// <param name="allow">Specifically allowed permissions.</param>
        /// <param name="deny">Specifically denied permissions.</param>
        /// <returns>Returns whether the operation was successful.</returns>
        public bool EditPermissions(DiscordGuildMember member, DiscordPermission allow, DiscordPermission deny)
        {
            try { return EditPermissionsAsync(member, allow, deny).Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Adds/edits a guild member permission overwrite for this channel.
        /// </summary>
        /// <param name="member">The member this overwrite will change permissions for.</param>
        /// <param name="allow">Specifically allowed permissions.</param>
        /// <param name="deny">Specifically denied permissions.</param>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> EditPermissionsAsync(DiscordGuildMember member, DiscordPermission allow, DiscordPermission deny)
        {
            return await EditPermissions(member.Id, allow, deny, DiscordOverwriteType.Member);
        }

        /// <summary>
        /// Adds/edits a role permission overwrite for this channel.
        /// </summary>
        /// <param name="role">The role this overwrite will change permissions for.</param>
        /// <param name="allow">Specifically allowed permissions.</param>
        /// <param name="deny">Specifically denied permissions.</param>
        /// <returns>Returns whether the operation was successful.</returns>
        public bool EditPermissions(DiscordRole role, DiscordPermission allow, DiscordPermission deny)
        {
            try { return EditPermissionsAsync(role, allow, deny).Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Adds/edits a role permission overwrite for this channel.
        /// </summary>
        /// <param name="role">The role this overwrite will change permissions for.</param>
        /// <param name="allow">Specifically allowed permissions.</param>
        /// <param name="deny">Specifically denied permissions.</param>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> EditPermissionsAsync(DiscordRole role, DiscordPermission allow, DiscordPermission deny)
        {
            return await EditPermissions(role.Id, allow, deny, DiscordOverwriteType.Role);
        }

        async Task<bool> EditPermissions(Snowflake overwriteId, DiscordPermission allow, DiscordPermission deny,
            DiscordOverwriteType type)
        {
            DiscordOverwrite ov;
            if (PermissionOverwrites.TryGetValue(overwriteId, out ov))
                // Invoking edit directly on the overwrite will cause changes to be reflected immediately.
                return ov.Edit(allow, deny);
            else
            {
                // This permission most likely does not exist yet, so we have no changes to
                // instantly reflect so perform a normal http call.
                DiscordApiData data = await channelsHttp.EditPermissions(Id, overwriteId, allow, deny, type);
                return data.IsNull;
            }
        }

        /// <summary>
        /// Deletes a permission overwrite for a guild member.
        /// </summary>
        public bool DeletePermission(DiscordGuildMember member)
        {
            try { return DeletePermissionAsync(member).Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Deletes a permission overwrite for a guild member.
        /// </summary>
        public async Task<bool> DeletePermissionAsync(DiscordGuildMember member)
        {
            return await DeletePermission(member.Id);
        }

        /// <summary>
        /// Deletes a permission overwrite for a role.
        /// </summary>
        public bool DeletePermission(DiscordRole role)
        {
            try { return DeletePermissionAsync(role).Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Deletes a permission overwrite for a role.
        /// </summary>
        public async Task<bool> DeletePermissionAsync(DiscordRole role)
        {
            return await DeletePermission(role.Id);
        }

        /// <summary>
        /// Deletes a permission overwrite.
        /// </summary>
        public bool DeletePermission(DiscordOverwrite overwrite)
        {
            try { return DeletePermissionAsync(overwrite).Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Deletes a permission overwrite.
        /// </summary>
        public async Task<bool> DeletePermissionAsync(DiscordOverwrite overwrite)
        {
            return await DeletePermission(overwrite.Id);
        }

        async Task<bool> DeletePermission(Snowflake overwriteId)
        {
            DiscordApiData data = await channelsHttp.DeletePermission(Id, overwriteId);
            if (data.IsNull)
            {
                // If successful, reflect changes immediately.
                PermissionOverwrites.Remove(overwriteId);
                RolePermissionOverwrites.Remove(overwriteId);
                MemberPermissionOverwrites.Remove(overwriteId);
                return true;
            }
            else
                return false;
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
                        () => new DiscordOverwrite(shard, this), o => o.Update(overwriteData));

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
