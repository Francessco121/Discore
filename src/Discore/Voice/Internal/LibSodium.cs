using System.Runtime.InteropServices;

namespace Discore.Voice.Internal
{
    static class LibSodium
    {
        [DllImport("libsodium", EntryPoint = "sodium_init", CallingConvention = CallingConvention.Cdecl)]
        static extern int SodiumInit();

        [DllImport("libsodium", EntryPoint = "crypto_secretbox_easy", CallingConvention = CallingConvention.Cdecl)]
        unsafe static extern int CryptoSecretboxEasy(byte* output, byte[] input, long inputLength, byte[] nonce, byte[] secret);

        public static bool Init()
        {
            // -1 = failure
            // 0 = success
            // 1 = already initialized
            return SodiumInit() >= 0;
        }

        public static unsafe int Encrypt(byte[] input, long inputLength, byte[] output, int outputOffset, byte[] nonce, byte[] secret)
        {
            fixed (byte* outPtr = output)
                return CryptoSecretboxEasy(outPtr + outputOffset, input, inputLength, nonce, secret);
        }
    }
}
