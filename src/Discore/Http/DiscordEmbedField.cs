using System;

namespace Discore.Http
{
    public class DiscordEmbedField : IDiscordSerializable
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

        public DiscordEmbedField(DiscordApiData data)
        {
            Name = data.GetString("name");
            Value = data.GetString("value");
            IsInline = data.GetBoolean("inline").Value;
        }

        public override string ToString()
        {
            return Name;
        }

        public DiscordApiData Serialize()
        {
            DiscordApiData data = DiscordApiData.CreateContainer();
            data.Set("name", Name);
            data.Set("value", Value);
            data.Set("inline", IsInline);
            return data;
        }
    }
}
