namespace Discore.Http
{
    /// <summary>
    /// A set of parameters defining a permission overwrite.
    /// </summary>
    public class OverwriteOptions
    {
        /// <summary>
        /// Gets the ID of the role or user that this overwrites.
        /// </summary>
        public Snowflake Id { get; }
        /// <summary>
        /// Gets the type the overwrite affects.
        /// </summary>
        public DiscordOverwriteType Type { get; }
        /// <summary>
        /// Gets or sets the allowed permissions to overwrite.
        /// </summary>
        public DiscordPermission Allow { get; set; }
        /// <summary>
        /// Gets or sets the denied permissions to overwrite.
        /// </summary>
        public DiscordPermission Deny { get; set; }

        public OverwriteOptions(Snowflake roleOrUserId, DiscordOverwriteType type)
        {
            Id = roleOrUserId;
            Type = type;
        }

        /// <summary>
        /// Sets the allowed permissions to overwrite.
        /// </summary>
        public OverwriteOptions SetAllowedPermissions(DiscordPermission allow)
        {
            Allow = allow;
            return this;
        }

        /// <summary>
        /// Sets the denied permissions to overwrite.
        /// </summary>
        public OverwriteOptions SetDeniedPermissions(DiscordPermission deny)
        {
            Deny = deny;
            return this;
        }

        internal DiscordApiData Build()
        {
            DiscordApiData data = new DiscordApiData(DiscordApiDataType.Container);
            data.SetSnowflake("id", Id);
            data.Set("type", Type.ToString().ToLower());
            data.Set("allow", (int)Allow);
            data.Set("deny", (int)Deny);

            return data;
        }
    }
}
