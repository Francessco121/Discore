using System;
using System.Text.Json;

#nullable enable

namespace Discore
{
    public sealed class DiscordReaction
    {
        /// <summary>
        /// Gets the number of times this emoji has been used to react.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets whether the current bot reacted using this emoji.
        /// </summary>
        public bool Me { get; }

        /// <summary>
        /// Gets the emoji information of this reaction.
        /// </summary>
        public DiscordReactionEmoji Emoji { get; }

        /// <exception cref="ArgumentNullException">Thrown if <paramref name="emoji"/> is null.</exception>
        public DiscordReaction(int count, bool me, DiscordReactionEmoji emoji)
        {
            Count = count;
            Me = me;
            Emoji = emoji ?? throw new ArgumentNullException(nameof(emoji));
        }

        internal DiscordReaction(JsonElement json)
        {
            Count = json.GetProperty("count").GetInt32();
            Me = json.GetProperty("me").GetBoolean();
            Emoji = new DiscordReactionEmoji(json.GetProperty("emoji"));
        }

        public override string ToString()
        {
            return Emoji.Name ?? base.ToString();
        }
    }
}

#nullable restore
