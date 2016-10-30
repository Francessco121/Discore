using System;

namespace Discore
{
    public class DiscordUser : DiscordIdObject
    {
        public string Username { get; private set; }

        /// <summary>
        /// Gets the user's 4-digit discord-tag.
        /// </summary>
        public string Discriminator { get; private set; }

        /// <summary>
        /// Gets user's avatar hash.
        /// </summary>
        public string Avatar { get; private set; }

        /// <summary>
        /// Gets whether this account belongs to an OAuth application.
        /// </summary>
        public bool IsBot { get; private set; }

        /// <summary>
        /// Gets whether this account has two-factor authentication enabled.
        /// </summary>
        public bool HasTwoFactorAuth { get; private set; }

        /// <summary>
        /// Gets whether the email on this account is verified.
        /// </summary>
        public bool IsVerified { get; private set; }

        /// <summary>
        /// Gets the email (if available) of this account.
        /// </summary>
        public string Email { get; private set; }

        /// <summary>
        /// Gets the game this user is currently playing.
        /// </summary>
        public DiscordGame Game { get; private set; }

        /// <summary>
        /// Gets the current status of this user.
        /// </summary>
        public DiscordUserStatus Status { get; private set; }

        internal override DiscordObject MemberwiseClone()
        {
            DiscordUser user = (DiscordUser)base.MemberwiseClone();
            user.Game = (DiscordGame)Game.MemberwiseClone();

            return user;
        }

        internal override void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Username = data.GetString("username") ?? Username;
            Discriminator = data.GetString("discriminator") ?? Discriminator;
            Avatar = data.GetString("avatar") ?? Avatar;
            IsVerified = data.GetBoolean("verified") ?? IsVerified;
            Email = data.GetString("email") ?? Email;

            DiscordApiData gameData = data.Get("game");
            if (gameData != null)
            {
                if (gameData.IsNull)
                    Game = null;
                else
                {
                    if (Game == null)
                        Game = new DiscordGame();

                    Game.Update(gameData);
                }
            }

            string statusStr = data.GetString("status");
            if (statusStr != null)
            {
                DiscordUserStatus status;
                if (Enum.TryParse(statusStr, true, out status))
                    Status = status;
            }
        }

        public override string ToString()
        {
            return Username;
        }
    }
}
