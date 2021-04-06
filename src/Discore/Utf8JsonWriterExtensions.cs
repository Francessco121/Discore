using System.Text.Json;

namespace Discore
{
    static class Utf8JsonWriterExtensions
    {
        public static void WriteSnowflake(this Utf8JsonWriter writer, string propertyName, Snowflake? value)
        {
            if (value == null)
                writer.WriteNull(propertyName);
            else
                writer.WriteString(propertyName, value.Value.ToString());
        }

        public static void WriteSnowflakeValue(this Utf8JsonWriter writer, Snowflake? value)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value.Value.ToString());
        }

        public static void WriteNumber(this Utf8JsonWriter writer, string propertyName, int? value)
        {
            if (value == null)
                writer.WriteNull(propertyName);
            else
                writer.WriteNumber(propertyName, value.Value);
        }
    }
}
