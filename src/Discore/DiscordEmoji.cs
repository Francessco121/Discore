using System.Collections.Generic;

namespace Discore
{
    public class DiscordEmoji : IDiscordObject, ICacheable
    {
        public DiscordGuild Guild { get; private set; }
        public string Id { get; private set; }
        public string Name { get; private set; }
        public DiscordRole[] Roles { get; private set; }
        public bool RequireColons { get; private set; }
        public bool Managed { get; private set; }

        DiscordApiCache cache;

        public DiscordEmoji(IDiscordClient client, DiscordGuild guild)
        {
            cache = client.Cache;
            Guild = guild;
        }

        public void Update(DiscordApiData data)
        {
            Id = data.GetString("id") ?? Id;
            Name = data.GetString("name") ?? Name;
            RequireColons = data.GetBoolean("require_colons") ?? RequireColons;
            Managed = data.GetBoolean("managed") ?? Managed;

            IReadOnlyList<DiscordApiData> roles = data.GetArray("roles");
            if (roles != null)
            {
                Roles = new DiscordRole[roles.Count];
                for (int i = 0; i < Roles.Length; i++)
                {
                    string roleId = roles[i].ToString();
                    DiscordRole role;
                    if (cache.TryGet(Guild, roleId, out role))
                        Roles[i] = role;
                    else
                        DiscordLogger.Default.LogWarning($"[EMOJI.UPDATE] Failed to find role with id {roleId} in "
                            + $"guild '{Guild.Name}'");
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
