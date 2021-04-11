using System;
using System.Text.Json;

namespace Discore
{
    static class JsonElementExtensions
    {
        public static bool HasProperty(this JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out _);
        }

        public static JsonElement? GetPropertyOrNull(this JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement value))
                return value;
            else
                return null;
        }

        public static Snowflake GetSnowflake(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Number)
                return element.GetUInt64();
            else
                return Snowflake.Parse(element.GetString()!);
        }

        public static Snowflake? GetSnowflakeOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetSnowflake();
        }

        public static bool? GetBooleanOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetBoolean();
        }

        public static byte? GetByteOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetByte();
        }

        public static DateTime? GetDateTimeOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetDateTime();
        }

        public static DateTimeOffset? GetDateTimeOffsetOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetDateTimeOffset();
        }

        public static decimal? GetDecimalOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetDecimal();
        }

        public static double? GetDoubleOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetDouble();
        }

        public static Guid? GetGuidOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetGuid();
        }

        public static short? GetInt16OrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetInt16();
        }

        public static int? GetInt32OrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetInt32();
        }

        public static long? GetInt64OrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetInt64();
        }

        public static sbyte? GetSByteOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetSByte();
        }

        public static float? GetSingleOrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetSingle();
        }

        public static ushort? GetUInt16OrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetUInt16();
        }

        public static uint? GetUInt32OrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetUInt32();
        }

        public static ulong? GetUInt64OrNull(this JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Null) return null;

            return element.GetUInt64();
        }
    }
}
