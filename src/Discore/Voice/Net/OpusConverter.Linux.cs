using System;
using System.Runtime.InteropServices;

namespace Discore.Voice.Net
{
    internal abstract partial class OpusConverter
    {
        public unsafe static partial class UnsafeNativeMethods
        {
            public class OpusLinux : IOpusConverter
            {
                IntPtr IOpusConverter.CreateEncoder(int Fs, int channels, int application, out OpusError error)
                {
                    return CreateEncoder(Fs, channels, application, out error);
                }

                void IOpusConverter.DestroyEncoder(IntPtr encoder)
                {
                    DestroyDecoder(encoder);
                }

                int IOpusConverter.Encode(IntPtr st, byte* pcm, int frame_size, byte[] data, int max_data_bytes)
                {
                    return Encode(st, pcm, frame_size, data, max_data_bytes);
                }

                int IOpusConverter.EncoderCtl(IntPtr st, OpusCtl request, int value)
                {
                    return EncoderCtl(st, request, value);
                }

                IntPtr IOpusConverter.CreateDecoder(int Fs, int channels, out OpusError error)
                {
                    return CreateDecoder(Fs, channels, out error);
                }

                void IOpusConverter.DestroyDecoder(IntPtr decoder)
                {
                    DestroyDecoder(decoder);
                }

                int IOpusConverter.Decode(IntPtr st, byte* data, int len, byte[] pcm, int frame_size, int decode_fec)
                {
                    return Decode(st, data, len, pcm, frame_size, decode_fec);
                }

                const string lib = "libopus.so.0";

                [DllImport(lib, EntryPoint = "opus_encoder_create", CallingConvention = CallingConvention.Cdecl)]
                static extern IntPtr CreateEncoder(int Fs, int channels, int application, out OpusError error);

                [DllImport(lib, EntryPoint = "opus_encoder_destroy", CallingConvention = CallingConvention.Cdecl)]
                static extern void DestroyEncoder(IntPtr encoder);

                [DllImport(lib, EntryPoint = "opus_encode", CallingConvention = CallingConvention.Cdecl)]
                static extern int Encode(IntPtr st, byte* pcm, int frame_size, byte[] data, int max_data_bytes);

                [DllImport(lib, EntryPoint = "opus_encoder_ctl", CallingConvention = CallingConvention.Cdecl)]
                static extern int EncoderCtl(IntPtr st, OpusCtl request, int value);

                [DllImport(lib, EntryPoint = "opus_decoder_create", CallingConvention = CallingConvention.Cdecl)]
                static extern IntPtr CreateDecoder(int Fs, int channels, out OpusError error);

                [DllImport(lib, EntryPoint = "opus_decoder_destroy", CallingConvention = CallingConvention.Cdecl)]
                static extern void DestroyDecoder(IntPtr decoder);

                [DllImport(lib, EntryPoint = "opus_decode", CallingConvention = CallingConvention.Cdecl)]
                static extern int Decode(IntPtr st, byte* data, int len, byte[] pcm, int frame_size, int decode_fec);
            }
        }
    }
}
