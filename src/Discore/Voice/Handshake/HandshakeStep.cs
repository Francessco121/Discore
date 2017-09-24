using System.Threading.Tasks;

namespace Discore.Voice.Handshake
{
    delegate Task HandshakeStep<T>(T state, DiscoreLogger log);
}
