using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Discore
{
    public class DiscordGuildMember : DiscordIdObject
    {
        public DiscordUser User { get { return cache.Get<DiscordUser>(userId); } }

        /// <summary>
        /// Gets the guild-wide nickname of the user.
        /// </summary>
        public string Nickname { get; private set; }

        string userId;
        DiscordApiCache cache;

        internal DiscordGuildMember(DiscordApiCache cache)
        {
            this.cache = cache;
        }

        internal override void Update(DiscordApiData data)
        {
            Nickname = data.GetString("nick") ?? Nickname;
            userId = data.LocateString("user.id") ?? userId;
        }
    }
}
