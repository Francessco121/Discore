namespace Discore.Http
{
    /// <summary>
    /// A permission overwrite for a <see cref="DiscordRole"/> or <see cref="DiscordGuildMember"/>.
    /// </summary>
    public class DiscordOverwrite : DiscordIdObject
    {
        /// <summary>
        /// The type of this overwrite.
        /// </summary>
        public DiscordOverwriteType Type { get; }
        /// <summary>
        /// The specifically allowed permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Allow { get; }
        /// <summary>
        /// The specifically denied permissions specified by this overwrite.
        /// </summary>
        public DiscordPermission Deny { get; }

        public DiscordOverwrite(DiscordApiData data)
            : base(data)
        {
            string type = data.GetString("type");
            switch (type)
            {
                case "role":
                    Type = DiscordOverwriteType.Role;
                    break;
                case "member":
                    Type = DiscordOverwriteType.Member;
                    break;
            }

            long allow = data.GetInt64("allow").Value;
            Allow = (DiscordPermission)allow;

            long deny = data.GetInt64("deny").Value;
            Deny = (DiscordPermission)deny;
        }

        public override string ToString()
        {
            return $"{Type} Overwrite: {Id}";
        }
    }
}
