using System;

namespace Discore.Voice.Internal
{
    internal unsafe interface IOpusConverter
    {
        IntPtr CreateEncoder(int Fs, int channels, int application, out OpusError error);
        void DestroyEncoder(IntPtr encoder);
        int Encode(IntPtr st, byte* pcm, int frame_size, byte[] data, int max_data_bytes);
        int EncoderCtl(IntPtr st, OpusCtl request, int value);
        IntPtr CreateDecoder(int Fs, int channels, out OpusError error);
        void DestroyDecoder(IntPtr decoder);
        int Decode(IntPtr st, byte* data, int len, byte[] pcm, int frame_size, int decode_fec);
    }
}
