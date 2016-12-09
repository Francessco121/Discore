namespace Discore
{
    public sealed class DiscordReaction
    {
        /// <summary>
        /// Gets the number of times this emoji has been used to react.
        /// </summary>
        public int Count { get; }

        /// <summary>
        /// Gets whether the current authenticated user reacted using this emoji.
        /// </summary>
        public bool Me { get; }

        /// <summary>
        /// Gets the emoji information of this reaction.
        /// </summary>
        public DiscordReactionEmoji Emoji { get; }

        internal DiscordReaction(DiscordApiData data)
        {
            Count = data.GetInteger("count").Value;
            Me = data.GetBoolean("me").Value;

            DiscordApiData emojiData = data.Get("emoji");
            if (emojiData != null)
                Emoji = new DiscordReactionEmoji(emojiData);
        }

        public override string ToString()
        {
            return Emoji == null ? base.ToString() : Emoji.Name;
        }
    }
}
