using System;

namespace Discore.WebSocket
{
    public sealed class DiscordUser : DiscordIdObject
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

        internal DiscordUser() { }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Username         = data.GetString("username") ?? Username;
            Discriminator    = data.GetString("discriminator") ?? Discriminator;
            Avatar           = data.GetString("avatar") ?? Avatar;
            IsVerified       = data.GetBoolean("verified") ?? IsVerified;
            Email            = data.GetString("email") ?? Email;
            IsBot            = data.GetBoolean("bot") ?? IsBot;
            HasTwoFactorAuth = data.GetBoolean("mfa_enabled") ?? HasTwoFactorAuth;
        }

        /// <summary>
        /// Gets whether or not this user has the specified permissions.
        /// </summary>
        /// <param name="permission">The permissions to check.</param>
        /// <param name="guild">The guild to check permissions for.</param>
        /// <returns>Returns whether or not the user has permission.</returns>
        /// <exception cref="ArgumentException">Thrown if the user is not in the specified guild.</exception>
        public bool HasPermission(DiscordPermission permission, DiscordGuild guild)
        {
            DiscordGuildMember member;
            if (guild.Members.TryGetValue(Id, out member))
                return member.HasPermission(permission);

            throw new ArgumentException("User is not in the specified guild", "guild");
        }

        /// <summary>
        /// Gets whether or not this user has the specified permissions in the context of
        /// the specified <see cref="DiscordGuildChannel"/>.
        /// </summary>
        /// <param name="permission">The permissions to check.</param>
        /// <param name="forChannel">The channel to check permissions for.</param>
        /// <returns>Returns whether or not the user has permission.</returns>
        /// <exception cref="ArgumentException">Thrown if the user is not in the specified guild.</exception>
        public bool HasPermission(DiscordPermission permission, DiscordGuildChannel forChannel)
        {
            DiscordGuildMember member;
            if (forChannel.Guild.Members.TryGetValue(Id, out member))
                return member.HasPermission(permission, forChannel);

            throw new ArgumentException("User is not in the specified guild", "channel");
        }

        /// <summary>
        /// Checks if this user has the specified permissions,
        /// if not a <see cref="DiscordPermissionException"/> is thrown.
        /// </summary>
        /// <param name="permission">The permissions to check.</param>
        /// <param name="guild">The guild to check permissions for.</param>
        /// <exception cref="ArgumentException">Thrown if the user is not in the specified guild.</exception>
        /// <exception cref="DiscordPermissionException">Thrown if the user does not have the specified permissions.</exception>
        public void AssertPermission(DiscordPermission permission, DiscordGuild guild)
        {
            DiscordGuildMember member;
            if (guild.Members.TryGetValue(Id, out member))
                member.AssertPermission(permission);
            else
                throw new ArgumentException("User is not in the specified guild", "guild");
        }

        /// <summary>
        /// Checks if this user has the specified permissions in the context 
        /// of the specified <see cref="DiscordGuildChannel"/>,
        /// if not a <see cref="DiscordPermissionException"/> is thrown.
        /// </summary>
        /// <param name="permission">The permissions to check.</param>
        /// <param name="channel">The channel to check permissions for.</param>
        /// <exception cref="ArgumentException">Thrown if the user is not in the specified guild.</exception>
        /// <exception cref="DiscordPermissionException">Thrown if the user does not have the specified permissions.</exception>
        public void AssertPermission(DiscordPermission permission, DiscordGuildChannel channel)
        {
            DiscordGuildMember member;
            if (channel.Guild.Members.TryGetValue(Id, out member))
                member.AssertPermission(permission, channel);
            else
                throw new ArgumentException("User is not in the specified guild", "channel");
        }

        internal void PresenceUpdate(DiscordApiData data)
        {
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
