namespace Discore
{
    public struct DiscordColor
    {
        public byte R;
        public byte G;
        public byte B;

        public DiscordColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }

        public int ToHexadecimal()
        {
            return R << 16
                | G << 8
                | B << 0;
        }

        public static DiscordColor FromHexadecimal(int hex)
        {
            byte r = (byte)(hex >> 16);
            byte g = (byte)(hex >> 8);
            byte b = (byte)(hex >> 0);

            return new DiscordColor(r, g, b);
        }
    }
}
