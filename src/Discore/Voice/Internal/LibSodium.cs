using System.Runtime.InteropServices;

namespace Discore.Voice.Internal
{
    static class LibSodium
    {
        [DllImport("libsodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
        unsafe static extern int SecretBoxEasy(byte* output, byte[] input, long inputLength, byte[] nonce, byte[] secret);

        public static unsafe int Encrypt(byte[] input, long inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
        {
            fixed (byte* outPtr = output)
                return SecretBoxEasy(outPtr + outputOffset, input, inputLength, nonce, secret);
        }
    }
}
