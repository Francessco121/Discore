using System;

namespace Discore
{
    public class DiscordioException : Exception
    {
        public DiscordioException(string message)
            : base(message)
        { }
    }
}
