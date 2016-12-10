using Discore.Http;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore
{
    public abstract class DiscordGuildChannel : DiscordChannel
    {
        /// <summary>
        /// Gets the type of guild channel (text or voice).
        /// </summary>
        public DiscordGuildChannelType GuildChannelType { get; }

        /// <summary>
        /// Gets the name of this channel.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the UI ordering position of this channel.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Gets a list of all permission overwrites associated with this channel.
        /// </summary>
        public IReadOnlyList<DiscordOverwrite> PermissionOverwrites { get; }

        /// <summary>
        /// Gets the id of the guild this channel is in.
        /// </summary>
        public Snowflake GuildId { get; }

        DiscordHttpChannelsEndpoint channelsHttp;

        internal DiscordGuildChannel(IDiscordApplication app, DiscordApiData data, DiscordGuildChannelType type, 
            Snowflake? guildId) 
            : base(app, data, DiscordChannelType.Guild)
        {
            channelsHttp = app.HttpApi.Channels;

            GuildChannelType = type;

            GuildId = guildId ?? data.GetSnowflake("guild_id").Value;
            Name = data.GetString("name");
            Position = data.GetInteger("position").Value;

            IList<DiscordApiData> overwrites = data.GetArray("permission_overwrites");
            DiscordOverwrite[] permissionOverwrites = new DiscordOverwrite[overwrites.Count];

            for (int i = 0; i < overwrites.Count; i++)
                permissionOverwrites[i] = new DiscordOverwrite(app, Id, overwrites[i]);

            PermissionOverwrites = permissionOverwrites;
        }

        /// <summary>
        /// Gets a list of all invites for this channel.
        /// </summary>
        public IReadOnlyList<DiscordInviteMetadata> GetInvites()
        {
            try { return GetInvitesAsync().Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Gets a list of all invites for this channel.
        /// </summary>
        public async Task<IReadOnlyList<DiscordInviteMetadata>> GetInvitesAsync()
        {
            return await channelsHttp.GetInvites(Id);
        }

        /// <summary>
        /// Creates an invite to this guild, through this channel.
        /// </summary>
        /// <param name="maxAge">Duration of invite before expiry, or 0 or null for never.</param>
        /// <param name="maxUses">Max number of uses or 0 or null for unlimited.</param>
        /// <param name="temporary">Whether this invite only grants temporary membership.</param>
        /// <param name="unique">If true, don't try to reuse a similar invite (useful for creating many unique one time use invites).</param>
        public DiscordInvite CreateInvite(TimeSpan? maxAge = null, int? maxUses = null,
            bool? temporary = null, bool? unique = null)
        {
            try { return CreateInviteAsync(maxAge, maxUses, temporary, unique).Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Creates an invite to this guild, through this channel.
        /// </summary>
        /// <param name="maxAge">Duration of invite before expiry, or 0 or null for never.</param>
        /// <param name="maxUses">Max number of uses or 0 or null for unlimited.</param>
        /// <param name="temporary">Whether this invite only grants temporary membership.</param>
        /// <param name="unique">If true, don't try to reuse a similar invite (useful for creating many unique one time use invites).</param>
        public async Task<DiscordInvite> CreateInviteAsync(TimeSpan? maxAge = null, int? maxUses = null, 
            bool? temporary = null, bool? unique = null)
        {
            return await channelsHttp.CreateInvite(Id, maxAge, maxUses, temporary, unique);
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
            return await channelsHttp.EditPermissions(Id, overwriteId, allow, deny, type);
        }

        /// <summary>
        /// Deletes a permission overwrite for a guild member.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public bool DeletePermission(DiscordGuildMember member)
        {
            try { return DeletePermissionAsync(member).Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Deletes a permission overwrite for a guild member.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> DeletePermissionAsync(DiscordGuildMember member)
        {
            return await DeletePermission(member.Id);
        }

        /// <summary>
        /// Deletes a permission overwrite for a role.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public bool DeletePermission(DiscordRole role)
        {
            try { return DeletePermissionAsync(role).Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Deletes a permission overwrite for a role.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> DeletePermissionAsync(DiscordRole role)
        {
            return await DeletePermission(role.Id);
        }

        /// <summary>
        /// Deletes a permission overwrite.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public bool DeletePermission(DiscordOverwrite overwrite)
        {
            try { return DeletePermissionAsync(overwrite).Result; }
            catch (AggregateException aex) { throw aex.InnerException; }
        }

        /// <summary>
        /// Deletes a permission overwrite.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public async Task<bool> DeletePermissionAsync(DiscordOverwrite overwrite)
        {
            return await DeletePermission(overwrite.Id);
        }

        async Task<bool> DeletePermission(Snowflake overwriteId)
        {
            return await channelsHttp.DeletePermission(Id, overwriteId);
        }

        public override string ToString()
        {
            return $"{GuildChannelType} Channel: {Name}";
        }
    }
}
