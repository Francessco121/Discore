using System;
using System.Threading;

namespace Discore.Audio
{
    // Shamefully stolen from
    // https://github.com/RogueException/Discord.Net/blob/a357a06d33c5631f5fb7181e7e231d960bf7a3f3/src/Discord.Net.Audio/VoiceBuffer.cs
    // for now...
    internal class VoiceBuffer
    {
        private readonly int _frameSize, _frameCount, _bufferSize;
        private readonly byte[] _buffer;
        private readonly byte[] _blankFrame;
        private ushort _readCursor, _writeCursor;
        private ManualResetEventSlim _notOverflowEvent;
        private bool _isClearing;
        private object _lock;

        public int FrameSize => _frameSize;
        public int FrameCount => _frameCount;
        public ushort ReadPos => _readCursor;
        public ushort WritePos => _writeCursor;

        public VoiceBuffer(int frameCount, int frameSize)
        {
            _frameSize = frameSize;
            _frameCount = frameCount;
            _bufferSize = _frameSize * _frameCount;
            _readCursor = 0;
            _writeCursor = 0;
            _buffer = new byte[_bufferSize];
            _blankFrame = new byte[_frameSize];
            _notOverflowEvent = new ManualResetEventSlim(); //Notifies when an overflow is solved
            _lock = new object();
        }

        public void Push(byte[] buffer, int offset, int count, CancellationToken cancelToken)
        {
            if (cancelToken.IsCancellationRequested)
                throw new OperationCanceledException("Client is disconnected.", cancelToken);

            int wholeFrames = count / _frameSize;
            int expectedBytes = wholeFrames * _frameSize;
            int lastFrameSize = count - expectedBytes;

            lock (_lock)
            {
                for (int i = 0, pos = offset; i <= wholeFrames; i++, pos += _frameSize)
                {
                    //If the read cursor is in the next position, wait for it to move.
                    ushort nextPosition = _writeCursor;
                    AdvanceCursorPos(ref nextPosition);
                    if (_readCursor == nextPosition)
                    {
                        _notOverflowEvent.Reset();
                        try
                        {
                            _notOverflowEvent.Wait(cancelToken);
                        }
                        catch (OperationCanceledException ex)
                        {
                            throw new OperationCanceledException("Client is disconnected.", ex, cancelToken);
                        }
                    }

                    if (i == wholeFrames)
                    {
                        //If there are no partial frames, skip this step
                        if (lastFrameSize == 0)
                            break;

                        //Copy partial frame
                        Buffer.BlockCopy(buffer, pos, _buffer, _writeCursor * _frameSize, lastFrameSize);

                        //Wipe the end of the buffer
                        Buffer.BlockCopy(_blankFrame, 0, _buffer, _writeCursor * _frameSize + lastFrameSize, _frameSize - lastFrameSize);
                    }
                    else
                    {
                        //Copy full frame
                        Buffer.BlockCopy(buffer, pos, _buffer, _writeCursor * _frameSize, _frameSize);
                    }

                    //Advance the write cursor to the next position
                    AdvanceCursorPos(ref _writeCursor);
                }
            }
        }

        public bool Pop(byte[] buffer)
        {
            //using (_lock.Lock())
            //{
            if (_writeCursor == _readCursor)
            {
                _notOverflowEvent.Set();
                return false;
            }

            bool isClearing = _isClearing;
            if (!isClearing)
                Buffer.BlockCopy(_buffer, _readCursor * _frameSize, buffer, 0, _frameSize);

            //Advance the read cursor to the next position
            AdvanceCursorPos(ref _readCursor);
            _notOverflowEvent.Set();
            return !isClearing;
            //}
        }

        public void Clear(CancellationToken cancelToken)
        {
            lock (_lock)
            {
                _isClearing = true;
                for (int i = 0; i < _frameCount; i++)
                    Buffer.BlockCopy(_blankFrame, 0, _buffer, i * _frameCount, i++);

                _writeCursor = 0;
                _readCursor = 0;
                _isClearing = false;
            }
        }

        public void Wait(CancellationToken cancelToken)
        {
            while (true)
            {
                _notOverflowEvent.Wait(cancelToken);
                if (_writeCursor == _readCursor)
                    break;
            }
        }

        private void AdvanceCursorPos(ref ushort pos)
        {
            pos++;
            if (pos == _frameCount)
                pos = 0;
        }
    }
}
