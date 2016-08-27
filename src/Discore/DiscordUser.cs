using System;

namespace Discore
{
    /// <summary>
    /// Represents a user account in Discord.
    /// </summary>
    public class DiscordUser : IDiscordObject, ICacheable
    {
        /// <summary>
        /// Gets the id of this user.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Gets the name of this user.
        /// </summary>
        public string Username { get; private set; }
        /// <summary>
        /// Gets the discriminator of this user.
        /// </summary>
        public string Discriminator { get; private set; }
        /// <summary>
        /// Gets the avatar hash of this user.
        /// </summary>
        public string Avatar { get; private set; }
        /// <summary>
        /// Gets whether or not this user is verified.
        /// </summary>
        public bool Verified { get; private set; }
        /// <summary>
        /// Gets this users email (if public).
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
            if (guild.TryGetMember(Id, out member))
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
            if (forChannel.Guild.TryGetMember(Id, out member))
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
            if (guild.TryGetMember(Id, out member))
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
            if (channel.Guild.TryGetMember(Id, out member))
                member.AssertPermission(permission, channel);
            else
                throw new ArgumentException("User is not in the specified guild", "channel");
        }

        /// <summary>
        /// Updates this user with the specified <see cref="DiscordApiData"/>.
        /// </summary>
        /// <param name="data">The data to update this user with.</param>
        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Username = data.GetString("username") ?? Username;
            Discriminator = data.GetString("discriminator") ?? Discriminator;
            Avatar = data.GetString("avatar") ?? Avatar;
            Verified = data.GetBoolean("verified") ?? Verified;
            Email = data.GetString("email") ?? Email;

            DiscordApiData gameData = data.Get("game");
            if (gameData != null)
            {
                if (Game == null)
                    Game = new DiscordGame();

                Game.Update(gameData);
            }

            string status = data.GetString("status");
            if (status != null)
            {
                switch (status)
                {
                    case "online":
                        Status = DiscordUserStatus.Online;
                        break;
                    case "offline":
                        Status = DiscordUserStatus.Offline;
                        break;
                    case "idle":
                        Status = DiscordUserStatus.Idle;
                        break;
                }
            }
        }

        /// <summary>
        /// Gets the name of this user.
        /// </summary>
        /// <returns>Returns the name of this user.</returns>
        public override string ToString()
        {
            return Username;
        }
    }
}
