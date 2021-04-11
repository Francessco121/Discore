namespace Discore
{
    /// <summary>
    /// A structure representing color data from the Discord API.
    /// </summary>
    public struct DiscordColor
    {
        /// <summary>
        /// The default color used by Discord for embeds.
        /// </summary>
        public static DiscordColor EmbedDefault => FromHexadecimal(0x4f545c);

        /// <summary>
        /// The red component of the color.
        /// </summary>
        public byte R;
        /// <summary>
        /// The green component of the color.
        /// </summary>
        public byte G;
        /// <summary>
        /// The blue component of the color.
        /// </summary>
        public byte B;

        /// <summary>
        /// Creates a new <see cref="DiscordColor"/> instance.
        /// </summary>
        /// <param name="r">The red component.</param>
        /// <param name="g">The green component.</param>
        /// <param name="b">The blue component.</param>
        public DiscordColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Packs this <see cref="DiscordColor"/> into an <see cref="int"/> representing a hexadecimal number.
        /// </summary>
        /// <returns>Returns the packed color.</returns>
        public int ToHexadecimal()
        {
            return R << 16
                | G << 8
                | B << 0;
        }

        /// <summary>
        /// Converts an <see cref="int"/> containing a hexadecimal number into a <see cref="DiscordColor"/>.
        /// </summary>
        /// <param name="hex">The hexadecimal <see cref="int"/>.</param>
        /// <returns>Returns the converted color.</returns>
        public static DiscordColor FromHexadecimal(int hex)
        {
            unchecked
            {
                byte r = (byte)(hex >> 16);
                byte g = (byte)(hex >> 8);
                byte b = (byte)(hex >> 0);

                return new DiscordColor(r, g, b);
            }
        }

        public override string ToString()
        {
            return $"{R}, {G}, {B}";
        }
    }
}
