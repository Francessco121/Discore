using Discore.Http;
using System.Threading.Tasks;

namespace Discore
{
    /// <summary>
    /// Roles represent a set of permissions attached to a group of users. Roles have unique names, 
    /// colors, and can be "pinned" to the side bar, causing their members to be listed separately. 
    /// Roles are unique per guild, and can have separate permission profiles for the global 
    /// context (guild) and channel context.
    /// </summary>
    public sealed class DiscordRole : DiscordIdObject
    {
        /// <summary>
        /// Gets the id of the guild this role is for.
        /// </summary>
        public Snowflake GuildId { get; }
        /// <summary>
        /// Gets the name of this role.
        /// </summary>
        public string Name { get; }
        /// <summary>
        /// Gets the displayed color of this role.
        /// </summary>
        public DiscordColor Color { get; }
        /// <summary>
        /// Gets whether this role is pinned in the user list of a guild.
        /// </summary>
        public bool IsHoisted { get; }
        /// <summary>
        /// Gets the ordering position of this role.
        /// </summary>
        public int Position { get; }
        /// <summary>
        /// Gets the permissions specified by this role.
        /// </summary>
        public DiscordPermission Permissions { get; }
        /// <summary>
        /// Gets whether this role is managed.
        /// </summary>
        public bool IsManaged { get; }
        /// <summary>
        /// Gets whether this role is mentionable.
        /// </summary>
        public bool IsMentionable { get; }

        DiscordHttpGuildEndpoint guildsHttp;

        internal DiscordRole(IDiscordApplication app, Snowflake guildId, DiscordApiData data)
            : base(data)
        {
            guildsHttp = app.HttpApi.Guilds;

            GuildId = guildId;

            Name = data.GetString("name");
            IsHoisted = data.GetBoolean("hoist").Value;
            Position = data.GetInteger("position").Value;
            IsManaged = data.GetBoolean("managed").Value;
            IsMentionable = data.GetBoolean("mentionable").Value;

            int color = data.GetInteger("color").Value;
            Color = DiscordColor.FromHexadecimal(color);

            long permissions = data.GetInt64("permissions").Value;
            Permissions = (DiscordPermission)permissions;
        }

        /// <summary>
        /// Modifies the settings of this role.
        /// </summary>
        public Task<DiscordRole> Modify(ModifyRoleParameters parameters)
        {
            return guildsHttp.ModifyRole(GuildId, Id, parameters);
        }

        /// <summary>
        /// Deletes this role.
        /// </summary>
        public Task<DiscordRole> Delete()
        {
            return guildsHttp.DeleteRole(GuildId, Id);
        }

        public override string ToString()
        {
            return Name;
        }

        internal DiscordApiData Serialize()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.Set("name", Name);
            data.Set("hoist", IsHoisted);
            data.Set("position", Position);
            data.Set("managed", IsManaged);
            data.Set("mentionable", IsMentionable);
            data.Set("color", Color.ToHexadecimal());
            data.Set("permissions", (long)Permissions);

            return data;
        }
    }
}
