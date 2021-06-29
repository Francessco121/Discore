using System;
using System.Runtime.InteropServices;

namespace Discore.Voice.Internal
{
    internal enum OpusApplication : int
    {
        Voice = 2048,
        MusicOrMixed = 2049,
        LowLatency = 2051
    }

    internal enum OpusError : int
    {
        OK = 0,
        BadArg = -1,
        BufferToSmall = -2,
        InternalError = -3,
        InvalidPacket = -4,
        Unimplemented = -5,
        InvalidState = -6,
        AllocFail = -7
    }

    internal enum OpusCtl : int
    {
        SetBitrateRequest = 4002,
        GetBitrateRequest = 4003,
        SetInbandFECRequest = 4012,
        GetInbandFECRequest = 4013
    }

    internal abstract partial class OpusConverter : IDisposable
    {
        protected IntPtr _ptr;

        /// <summary> Gets the bit rate of this converter. </summary>
        public const int BitsPerSample = 16;
        /// <summary> Gets the input sampling rate of this converter. </summary>
        public int InputSamplingRate { get; }
        /// <summary> Gets the number of channels of this converter. </summary>
        public int InputChannels { get; }
        /// <summary> Gets the milliseconds per frame. </summary>
        public int FrameLength { get; }
        /// <summary> Gets the number of samples per frame. </summary>
        public int SamplesPerFrame { get; }
        /// <summary> Gets the bytes per frame. </summary>
        public int FrameSize { get; }
        /// <summary> Gets the bytes per sample. </summary>
        public int SampleSize { get; }

        public IOpusConverter Opus { get; }

        protected OpusConverter(int samplingRate, int channels, int frameLength)
        {
            string os = RuntimeInformation.OSDescription.ToLower();

            if (os.Contains("linux"))
            { //Linux*
                Opus = new UnsafeNativeMethods.OpusLinux();
            }
            else if (os.Contains("windows"))
            { //Microsoft
                Opus = new UnsafeNativeMethods.OpusWindows();
            }
            else if (os.Contains("darwin"))
            { //Mac
                Opus = new UnsafeNativeMethods.OpusDarwin();
            }
            else
            {
                throw new PlatformNotSupportedException($"{os} isn't supported currently");
            }

            if (samplingRate != 8000 && samplingRate != 12000 &&
                samplingRate != 16000 && samplingRate != 24000 &&
                samplingRate != 48000)
                throw new ArgumentOutOfRangeException(nameof(samplingRate));

            if (channels != 1 && channels != 2)
                throw new ArgumentOutOfRangeException(nameof(channels));

            InputSamplingRate = samplingRate;
            InputChannels = channels;
            FrameLength = frameLength;
            SampleSize = (BitsPerSample / 8) * channels;
            SamplesPerFrame = samplingRate / 1000 * FrameLength;
            FrameSize = SamplesPerFrame * SampleSize;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
                disposedValue = true;
        }

        ~OpusConverter()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
