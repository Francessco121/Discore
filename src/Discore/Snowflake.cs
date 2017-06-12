using System;

namespace Discore
{
    /// <summary>
    /// Twitter's snowflake format. Used for ID's in the Discord Api.
    /// https://github.com/twitter/snowflake/tree/snowflake-2010
    /// </summary>
    public struct Snowflake
    {
        /// <summary>
        /// Gets a snowflake representing nothing (or zero).
        /// <para>
        /// This can be used when modifying objects such as guilds (e.g. setting/removing an AFK channel). 
        /// </para>
        /// <para>
        /// Leaving properties null when modifying a guild will cause them to be left unchanged. 
        /// However, <see cref="None"/> will clear the value (i.e. removing the AFK channel).
        /// </para>
        /// </summary>
        public static readonly Snowflake None = new Snowflake(0);

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
            if (obj == null)
                return false;
            else if (typeof(Snowflake) == obj.GetType())
            {
                Snowflake other = (Snowflake)obj;
                return Id == other.Id;
            }
            else
                return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Parses a snowflake from a string.
        /// </summary>
        public static Snowflake Parse(string snowflakeString)
        {
            return new Snowflake(ulong.Parse(snowflakeString));
        }

        /// <summary>
        /// Attempts to parses a snowflake from a string,
        /// returns null if the parse failed.
        /// </summary>
        public static Snowflake? ParseOrNull(string snowflakeString)
        {
            ulong snowflakeId;
            if (ulong.TryParse(snowflakeString, out snowflakeId))
                return new Snowflake(snowflakeId);
            else
                return null;
        }

        /// <summary>
        /// Attempts to parse a snowflake from a string.
        /// </summary>
        public static bool TryParse(string snowflakeString, out Snowflake snowflake)
        {
            ulong snowflakeId;
            if (ulong.TryParse(snowflakeString, out snowflakeId))
            {
                snowflake = new Snowflake(snowflakeId);
                return true;
            }
            else
            {
                snowflake = default(Snowflake);
                return false;
            }
        }

        public static bool operator ==(Snowflake a, Snowflake b)
        {
            return a.Id == b.Id;
        }

        public static bool operator !=(Snowflake a, Snowflake b)
        {
            return a.Id != b.Id;
        }

        public static implicit operator ulong(Snowflake snowFlake)
        {
            return snowFlake.Id;
        }

        public static implicit operator Snowflake(ulong id)
        {
            return new Snowflake(id);
        }
    }
}
