#nullable enable

using System.Text.Json;

namespace Discore
{
    public sealed class DiscordEmbedField
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

        public DiscordEmbedField(string name, string value, bool isInline)
        {
            Name = name;
            Value = value;
            IsInline = isInline;
        }

        internal DiscordEmbedField(JsonElement json)
        {
            Name = json.GetProperty("name").GetString()!;
            Value = json.GetProperty("value").GetString()!;
            IsInline = json.GetPropertyOrNull("inline")?.GetBoolean() ?? false;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}

#nullable restore
