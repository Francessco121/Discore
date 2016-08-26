using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Discore.Secrets
{
    public class SecretException : DiscoreException
    {
        public SecretException(string message) : base(message) {}
    }

    public class SecretNotFoundException : DiscoreException
    {
        public SecretNotFoundException(string name) : base($"{name} could not be found") {}
    }
}
