using System;

namespace Discore
{
    /// <summary>
    /// Twitter's snowflake format. Used for ID's in the Discord Api.
    /// https://github.com/twitter/snowflake/tree/snowflake-2010
    /// </summary>
    public struct Snowflake
    {
        public ulong Id;

        public Snowflake(ulong id)
        {
            Id = id;
        }

        // bits 64 - 22
        public DateTimeOffset GetTimestamp()
        {
            ulong timestamp = Id >> 22;
            timestamp += 1420070400000; // Add Discord Epoch (unix ms)

            return DateTimeOffset.FromUnixTimeMilliseconds((long)timestamp);
        }

        // bits 22 - 17
        public int GetWorkerId()
        {
            int id = unchecked((int)(Id >> 17));
            return id & 0x1F; // 0x1F = 11111b
        }

        // bits 17 - 12
        public int GetProcessId()
        {
            int id = unchecked((int)(Id >> 12));
            return id & 0x1F; // 0x1F = 11111b
        }

        // bits 12 - 0
        public int GetIncrement()
        {
            int id = unchecked((int)Id);
            return id & 0xFFF;
        }

        public override string ToString()
        {
            return Id.ToString();
        }

        public override bool Equals(object obj)
        {
            return Id.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(Snowflake a, Snowflake b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(Snowflake a, Snowflake b)
        {
            return a.Id != b.Id;
        }
    }
}
