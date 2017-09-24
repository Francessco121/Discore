using System.Collections.Generic;
using System.Threading.Tasks;

namespace Discore.Voice.Handshake
{
    class HandshakeProcess<T>
    {
        IEnumerable<HandshakeStep<T>> steps;

        public HandshakeProcess(IEnumerable<HandshakeStep<T>> steps)
        {
            this.steps = steps;
        }

        public async Task Execute(T state, DiscoreLogger log)
        {
            foreach (HandshakeStep<T> step in steps)
                await step(state, log);
        }
    }
}
