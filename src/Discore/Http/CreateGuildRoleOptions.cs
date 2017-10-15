namespace Discore.Http
{
    /// <summary>
    /// A set of parameters for creating roles when creating a new guild.
    /// </summary>
    public class CreateGuildRoleOptions : CreateRoleOptions
    {
        /// <summary>
        /// Gets or sets a temporary ID for the role. This allows channels also created with
        /// the guild to overwrite this role's permissions. 
        /// <para>
        /// This role's ID will be overwritten upon creation of the guild.
        /// </para>
        /// </summary>
        public Snowflake TemporaryId { get; set; }

        /// <param name="temporaryId">A temporary ID for the role, which will be overwritten upon guild creation.</param>
        public CreateGuildRoleOptions(Snowflake temporaryId)
        {
            TemporaryId = temporaryId;
        }

        /// <summary>
        /// Sets the name of the role to create.
        /// </summary>
        public new CreateGuildRoleOptions SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the permissions of the role to create.
        /// </summary>
        public new CreateGuildRoleOptions SetPermissions(DiscordPermission permissions)
        {
            Permissions = permissions;
            return this;
        }

        /// <summary>
        /// Sets the color of the role to create.
        /// </summary>
        public new CreateGuildRoleOptions SetColor(DiscordColor color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        /// Sets whether the created role should be displayed in the sidebar.
        /// </summary>
        public new CreateGuildRoleOptions SetHoisted(bool hoist)
        {
            IsHoisted = hoist;
            return this;
        }

        /// <summary>
        /// Sets whether the created role should be mentionable.
        /// </summary>
        public new CreateGuildRoleOptions SetMentionable(bool mentionable)
        {
            IsMentionable = mentionable;
            return this;
        }

        internal override DiscordApiData Build()
        {
            DiscordApiData data = base.Build();
            data.SetSnowflake("id", TemporaryId);

            return data;
        }
    }
}
