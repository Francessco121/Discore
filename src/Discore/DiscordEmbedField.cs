namespace Discore
{
    public sealed class DiscordEmbedField : IDiscordSerializable
    {
        /// <summary>
        /// Gets the name of the field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the value of the field.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets whether this field should display inline.
        /// </summary>
        public bool IsInline { get; }

        internal DiscordEmbedField(DiscordApiData data)
        {
            Name = data.GetString("name");
            Value = data.GetString("value");
            IsInline = data.GetBoolean("inline").Value;
        }

        public DiscordApiData Serialize()
        {
            DiscordApiData data = DiscordApiData.ContainerType;
            data.Set("name", Name);
            data.Set("value", Value);
            data.Set("inline", IsInline);
            return data;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
