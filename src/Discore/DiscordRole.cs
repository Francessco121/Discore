using System.Text.Json;

namespace Discore
{
    /// <summary>
    /// Roles represent a set of permissions attached to a group of users.
    /// </summary>
    public class DiscordRole : DiscordIdEntity
    {
        /// <summary>
        /// Gets the ID of the guild this role is for.
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

        // TODO: add tags

        public DiscordRole(
            Snowflake id,
            Snowflake guildId, 
            string name, 
            DiscordColor color, 
            bool isHoisted, 
            int position, 
            DiscordPermission permissions, 
            bool isManaged, 
            bool isMentionable)
            : base(id)
        {
            GuildId = guildId;
            Name = name;
            Color = color;
            IsHoisted = isHoisted;
            Position = position;
            Permissions = permissions;
            IsManaged = isManaged;
            IsMentionable = isMentionable;
        }

        internal DiscordRole(JsonElement json, Snowflake guildId)
            : base(json)
        {
            GuildId = guildId;
            Name = json.GetProperty("name").GetString()!;
            Color = DiscordColor.FromHexadecimal(json.GetProperty("color").GetInt32());
            IsHoisted = json.GetProperty("hoist").GetBoolean();
            Position = json.GetProperty("position").GetInt32();
            Permissions = (DiscordPermission)json.GetProperty("permissions").GetUInt64();
            IsManaged = json.GetProperty("managed").GetBoolean();
            IsMentionable = json.GetProperty("mentionable").GetBoolean();
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
