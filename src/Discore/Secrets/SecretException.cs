using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discore.Secrets
{
    public class SecretException : DiscordioException
    {
        public SecretException(string message) : base(message) {}
    }

    public class SecretNotFoundException : DiscordioException
    {
        public SecretNotFoundException(string name) : base($"{name} could not be found") {}
    }
}
