using System;

namespace Discore
{
    public class DiscordUser : IDiscordObject, ICacheable
    {
        public string Id { get; private set; }
        public string Username { get; private set; }
        public string Discriminator { get; private set; }
        public string Avatar { get; private set; }
        public bool Verified { get; private set; }
        public string Email { get; private set; }
        public DiscordGame Game { get; private set; }
        public DiscordUserStatus Status { get; private set; }

        public bool HasPermission(DiscordPermission permission, DiscordGuild guild)
        {
            DiscordGuildMember member;
            if (guild.TryGetMember(Id, out member))
                return member.HasPermission(permission);

            throw new ArgumentException("User is not in the specified guild", "guild");
        }

        public bool HasPermission(DiscordPermission permission, DiscordGuildChannel channel)
        {
            DiscordGuildMember member;
            if (channel.Guild.TryGetMember(Id, out member))
                return member.HasPermission(permission, channel);

            throw new ArgumentException("User is not in the specified guild", "channel");
        }

        public void AssertPermission(DiscordPermission permission, DiscordGuild guild)
        {
            DiscordGuildMember member;
            if (guild.TryGetMember(Id, out member))
                member.AssertPermission(permission, guild);
            else
                throw new ArgumentException("User is not in the specified guild", "guild");
        }

        public void AssertPermission(DiscordPermission permission, DiscordGuildChannel channel)
        {
            DiscordGuildMember member;
            if (channel.Guild.TryGetMember(Id, out member))
                member.AssertPermission(permission, channel);
            else
                throw new ArgumentException("User is not in the specified guild", "channel");
        }

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

        public override string ToString()
        {
            return Username;
        }
    }
}
