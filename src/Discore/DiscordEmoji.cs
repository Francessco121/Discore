using System.Collections.Generic;

namespace Discore
{
    public sealed class DiscordEmoji : DiscordIdObject
    {
        /// <summary>
        /// Gets the guild this emoji is for.
        /// </summary>
        public DiscordGuild Guild { get; private set; }
        /// <summary>
        /// Gets the name of this emoji.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Gets the associated roles with this emoji.
        /// </summary>
        public DiscordApiCacheIdSet<DiscordRole> Roles { get; }
        /// <summary>
        /// Gets whether or not colons are required around the emoji name to use it.
        /// </summary>
        public bool RequireColons { get; private set; }
        /// <summary>
        /// Gets whether or not this emoji is managed.
        /// </summary>
        public bool IsManaged { get; private set; }

        Shard shard;

        internal DiscordEmoji(Shard shard, DiscordGuild guild)
        {
            this.shard = shard;
            Guild = guild;

            Roles = new DiscordApiCacheIdSet<DiscordRole>(guild.Roles);
        }

        internal override void Update(DiscordApiData data)
        {
            base.Update(data);

            Name = data.GetString("name") ?? Name;
            RequireColons = data.GetBoolean("require_colons") ?? RequireColons;
            IsManaged = data.GetBoolean("managed") ?? IsManaged;

            IList<DiscordApiData> roles = data.GetArray("roles");
            if (roles != null)
            {
                Roles.Clear();
                foreach (DiscordApiData roleData in roles)
                {
                    string roleId = roleData.GetString("id");

                    // TODO: Figure out if these roles are seperate, or are actually just guild roles.

                    // Follow through with the "eventual consistency" and update the role,
                    // but only save the id on our end.
                    Guild.Roles.Edit(roleId, () => new DiscordRole(), role => role.Update(roleData));

                    Roles.Add(roleId);
                }
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
