namespace Discore
{
    /// <summary>
    /// A permission overwrite for a <see cref="DiscordRole"/> or <see cref="DiscordGuildMember"/>.
    /// </summary>
    public class DiscordOverwrite : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this permission overwrite.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// The type of this overwrite.
        /// </summary>
        public DiscordOverwriteType Type { get; private set; }
        /// <summary>
        /// The specifically allowed permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Allow { get; private set; }
        /// <summary>
        /// The specifically denied permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Deny { get; private set; }

        /// <summary>
        /// Updates this overwrite with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this overwrite with.</param>
        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;

            string type = data.GetString("type");
            if (type != null)
            {
                switch (type)
                {
                    case "role":
                        Type = DiscordOverwriteType.Role;
                        break;
                    case "member":
                        Type = DiscordOverwriteType.Member;
                        break;
                }
            }

            long? allow = data.GetInt64("allow");
            if (allow.HasValue)
                Allow = (DiscordPermission)allow.Value;

            long? deny = data.GetInt64("deny");
            if (deny.HasValue)
                Deny = (DiscordPermission)deny.Value;
        }
    }
}
