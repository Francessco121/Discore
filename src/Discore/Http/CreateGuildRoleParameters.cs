namespace Discore.Http
{
    /// <summary>
    /// A set of parameters for creating roles when creating a new guild.
    /// </summary>
    public class CreateGuildRoleParameters : CreateRoleParameters
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
        public CreateGuildRoleParameters(Snowflake temporaryId)
        {
            TemporaryId = temporaryId;
        }

        /// <summary>
        /// Sets the name of the role to create.
        /// </summary>
        public new CreateGuildRoleParameters SetName(string name)
        {
            Name = name;
            return this;
        }

        /// <summary>
        /// Sets the permissions of the role to create.
        /// </summary>
        public new CreateGuildRoleParameters SetPermissions(DiscordPermission permissions)
        {
            Permissions = permissions;
            return this;
        }

        /// <summary>
        /// Sets the color of the role to create.
        /// </summary>
        public new CreateGuildRoleParameters SetColor(DiscordColor color)
        {
            Color = color;
            return this;
        }

        /// <summary>
        /// Sets whether the created role should be displayed in the sidebar.
        /// </summary>
        public new CreateGuildRoleParameters SetHoisted(bool hoist)
        {
            IsHoisted = hoist;
            return this;
        }

        /// <summary>
        /// Sets whether the created role should be mentionable.
        /// </summary>
        public new CreateGuildRoleParameters SetMentionable(bool mentionable)
        {
            IsMentionable = mentionable;
            return this;
        }

        internal override DiscordApiData Build()
        {
            DiscordApiData data = base.Build();
            data.Set("id", TemporaryId);

            return data;
        }
    }
}
