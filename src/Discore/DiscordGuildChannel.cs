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
        /// Gets a dictionary of all permission overwrites associated with this channel.
        /// </summary>
        public IReadOnlyDictionary<Snowflake, DiscordOverwrite> PermissionOverwrites { get; }

        /// <summary>
        /// Gets the id of the guild this channel is in.
        /// </summary>
        public Snowflake GuildId { get; }

        DiscordHttpChannelEndpoint channelsHttp;

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
            Dictionary<Snowflake, DiscordOverwrite> permissionOverwrites = new Dictionary<Snowflake, DiscordOverwrite>();

            for (int i = 0; i < overwrites.Count; i++)
            {
                DiscordOverwrite overwrite = new DiscordOverwrite(app, Id, overwrites[i]);
                permissionOverwrites.Add(overwrite.Id, overwrite);
            }

            PermissionOverwrites = permissionOverwrites;
        }

        /// <summary>
        /// Gets a list of all invites for this channel.
        /// </summary>
        public Task<IReadOnlyList<DiscordInviteMetadata>> GetInvites()
        {
            return channelsHttp.GetInvites(Id);
        }

        /// <summary>
        /// Creates an invite to this guild, through this channel.
        /// </summary>
        /// <param name="maxAge">Duration of invite before expiry, or 0 or null for never.</param>
        /// <param name="maxUses">Max number of uses or 0 or null for unlimited.</param>
        /// <param name="temporary">Whether this invite only grants temporary membership.</param>
        /// <param name="unique">If true, don't try to reuse a similar invite (useful for creating many unique one time use invites).</param>
        public Task<DiscordInvite> CreateInvite(TimeSpan? maxAge = null, int? maxUses = null, 
            bool? temporary = null, bool? unique = null)
        {
            return channelsHttp.CreateInvite(Id, maxAge, maxUses, temporary, unique);
        }

        /// <summary>
        /// Adds/edits a guild member permission overwrite for this channel.
        /// </summary>
        /// <param name="member">The member this overwrite will change permissions for.</param>
        /// <param name="allow">Specifically allowed permissions.</param>
        /// <param name="deny">Specifically denied permissions.</param>
        /// <returns>Returns whether the operation was successful.</returns>
        public Task<bool> EditPermissions(DiscordGuildMember member, DiscordPermission allow, DiscordPermission deny)
        {
            return EditPermissions(member.Id, allow, deny, DiscordOverwriteType.Member);
        }

        /// <summary>
        /// Adds/edits a role permission overwrite for this channel.
        /// </summary>
        /// <param name="role">The role this overwrite will change permissions for.</param>
        /// <param name="allow">Specifically allowed permissions.</param>
        /// <param name="deny">Specifically denied permissions.</param>
        /// <returns>Returns whether the operation was successful.</returns>
        public Task<bool> EditPermissions(DiscordRole role, DiscordPermission allow, DiscordPermission deny)
        {
            return EditPermissions(role.Id, allow, deny, DiscordOverwriteType.Role);
        }

        Task<bool> EditPermissions(Snowflake overwriteId, DiscordPermission allow, DiscordPermission deny,
            DiscordOverwriteType type)
        {
            return channelsHttp.EditPermissions(Id, overwriteId, allow, deny, type);
        }

        /// <summary>
        /// Deletes a permission overwrite for a guild member.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public Task<bool> DeletePermission(DiscordGuildMember member)
        {
            return DeletePermission(member.Id);
        }

        /// <summary>
        /// Deletes a permission overwrite for a role.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public Task<bool> DeletePermission(DiscordRole role)
        {
            return DeletePermission(role.Id);
        }

        /// <summary>
        /// Deletes a permission overwrite.
        /// </summary>
        /// <returns>Returns whether the operation was successful.</returns>
        public Task<bool> DeletePermission(DiscordOverwrite overwrite)
        {
            return DeletePermission(overwrite.Id);
        }

        Task<bool> DeletePermission(Snowflake overwriteId)
        {
            return channelsHttp.DeletePermission(Id, overwriteId);
        }

        public override string ToString()
        {
            return $"{GuildChannelType} Channel: {Name}";
        }
    }
}
