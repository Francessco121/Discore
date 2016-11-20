namespace Discore.WebSocket
{
    public sealed class DiscordEmbedField : DiscordObject
    {
        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the value of the field.
        /// </summary>
        public string Value { get; private set; }

        /// <summary>
        /// Gets whether this field should display inline.
        /// </summary>
        public bool IsInline { get; private set; }

        internal DiscordEmbedField() { }

        internal override void Update(DiscordApiData data)
        {
            Name = data.GetString("name") ?? Name;
            Value = data.GetString("value") ?? Value;
            IsInline = data.GetBoolean("inline") ?? IsInline;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
