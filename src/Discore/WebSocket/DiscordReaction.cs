namespace Discore.WebSocket
{
    public sealed class DiscordReaction : DiscordObject
    {
        /// <summary>
        /// Gets the number of times this emoji has been used to react.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets whether the current authenticated user reacted using this emoji.
        /// </summary>
        public bool Me { get; private set; }

        /// <summary>
        /// Gets the emoji information of this reaction.
        /// </summary>
        public DiscordReactionEmoji Emoji { get; private set; }

        internal DiscordReaction() { }

        internal override void Update(DiscordApiData data)
        {
            Count = data.GetInteger("count") ?? Count;
            Me = data.GetBoolean("me") ?? Me;

            DiscordApiData emojiData = data.Get("emoji");
            if (emojiData != null)
            {
                Emoji = new DiscordReactionEmoji();
                Emoji.Update(emojiData);
            }
        }

        public override string ToString()
        {
            return Emoji == null ? base.ToString() : Emoji.Name;
        }
    }
}
