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

        DiscordHttpClient http;

        internal DiscordGuildChannel(DiscordHttpClient http, DiscordApiData data, DiscordGuildChannelType type, 
            Snowflake? guildId) 
            : base(http, data, DiscordChannelType.Guild)
        {
            this.http = http;

            GuildChannelType = type;

            GuildId = guildId ?? data.GetSnowflake("guild_id").Value;
            Name = data.GetString("name");
            Position = data.GetInteger("position").Value;

            IList<DiscordApiData> overwrites = data.GetArray("permission_overwrites");
            Dictionary<Snowflake, DiscordOverwrite> permissionOverwrites = new Dictionary<Snowflake, DiscordOverwrite>();

            for (int i = 0; i < overwrites.Count; i++)
            {
                DiscordOverwrite overwrite = new DiscordOverwrite(http, Id, overwrites[i]);
                permissionOverwrites.Add(overwrite.Id, overwrite);
            }

            PermissionOverwrites = permissionOverwrites;
        }

        /// <summary>
        /// Gets a list of all invites for this channel.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<IReadOnlyList<DiscordInviteMetadata>> GetInvites()
        {
            return http.GetChannelInvites(Id);
        }

        /// <summary>
        /// Creates an invite to this guild, through this channel.
        /// </summary>
        /// <param name="maxAge">Duration of invite before expiry, or 0 or null for never.</param>
        /// <param name="maxUses">Max number of uses or 0 or null for unlimited.</param>
        /// <param name="temporary">Whether this invite only grants temporary membership.</param>
        /// <param name="unique">If true, don't try to reuse a similar invite (useful for creating many unique one time use invites).</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task<DiscordInvite> CreateInvite(TimeSpan? maxAge = null, int? maxUses = null, 
            bool? temporary = null, bool? unique = null)
        {
            return http.CreateChannelInvite(Id, maxAge, maxUses, temporary, unique);
        }

        /// <summary>
        /// Adds/edits a guild member permission overwrite for this channel.
        /// </summary>
        /// <param name="member">The member this overwrite will change permissions for.</param>
        /// <param name="allow">Specifically allowed permissions.</param>
        /// <param name="deny">Specifically denied permissions.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task EditPermissions(DiscordGuildMember member, DiscordPermission allow, DiscordPermission deny)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            return EditPermissions(member.Id, DiscordOverwriteType.Member, allow, deny);
        }

        /// <summary>
        /// Adds/edits a role permission overwrite for this channel.
        /// </summary>
        /// <param name="role">The role this overwrite will change permissions for.</param>
        /// <param name="allow">Specifically allowed permissions.</param>
        /// <param name="deny">Specifically denied permissions.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task EditPermissions(DiscordRole role, DiscordPermission allow, DiscordPermission deny)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return EditPermissions(role.Id, DiscordOverwriteType.Role, allow, deny);
        }

        /// <summary>
        /// Adds/edits a guild member or role permission overwrite for this channel.
        /// </summary>
        /// <param name="memberOrRoleId">The ID of the member or role this overwrite will change permissions for.</param>
        /// <param name="overwriteType">Whether the permissions should affect a member or role.</param>
        /// <param name="allow">Specifically allowed permissions.</param>
        /// <param name="deny">Specifically denied permissions.</param>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task EditPermissions(Snowflake memberOrRoleId, DiscordOverwriteType overwriteType, 
            DiscordPermission allow, DiscordPermission deny)
        {
            return http.EditChannelPermissions(Id, memberOrRoleId, allow, deny, overwriteType);
        }

        /// <summary>
        /// Deletes a permission overwrite for a guild member.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeletePermission(DiscordGuildMember member)
        {
            if (member == null)
                throw new ArgumentNullException(nameof(member));

            return DeletePermission(member.Id);
        }

        /// <summary>
        /// Deletes a permission overwrite for a role.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeletePermission(DiscordRole role)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            return DeletePermission(role.Id);
        }

        /// <summary>
        /// Deletes a permission overwrite.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeletePermission(DiscordOverwrite overwrite)
        {
            if (overwrite == null)
                throw new ArgumentNullException(nameof(overwrite));

            return DeletePermission(overwrite.Id);
        }

        /// <summary>
        /// Deletes a permission overwrite.
        /// </summary>
        /// <exception cref="DiscordHttpApiException"></exception>
        public Task DeletePermission(Snowflake memberOrRoleId)
        {
            return http.DeleteChannelPermission(Id, memberOrRoleId);
        }

        public override string ToString()
        {
            return $"{GuildChannelType} Channel: {Name}";
        }
    }
}
