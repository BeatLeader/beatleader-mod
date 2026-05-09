using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeatLeader.Models.Replay;

public class BufferedReplayStreamReader {
    public const int DefaultBufferSize = 32 * 1024;

    private readonly Stream _stream;
    private byte[] _buffer;
    private int _offset;
    private int _count;

    public BufferedReplayStreamReader(Stream stream, byte[] buffer) {
        _stream = stream;
        _buffer = buffer;
    }
    
    private int Available => _count - _offset;

    public byte ReadByte() {
        EnsureAvailable(1);
        var result = _buffer[_offset++];
        if (_offset == _count) {
            _offset = 0;
            _count = 0;
        }
        return result;
    }

    public bool ReadBool() {
        return ReadByte() != 0;
    }

    public int ReadInt32() {
        EnsureAvailable(4);
        var result = BitConverter.ToInt32(_buffer, _offset);
        _offset += 4;
        if (_offset == _count) {
            _offset = 0;
            _count = 0;
        }
        return result;
    }

    public long ReadInt64() {
        EnsureAvailable(8);
        var result = BitConverter.ToInt64(_buffer, _offset);
        _offset += 8;
        if (_offset == _count) {
            _offset = 0;
            _count = 0;
        }
        return result;
    }

    public float ReadSingle() {
        EnsureAvailable(4);
        var result = BitConverter.ToSingle(_buffer, _offset);
        _offset += 4;
        if (_offset == _count) {
            _offset = 0;
            _count = 0;
        }
        return result;
    }

    public int PeekInt32(int relativeOffset) {
        if (relativeOffset < 0) {
            throw new ArgumentOutOfRangeException(nameof(relativeOffset));
        }
        
        EnsureAvailable(relativeOffset + 4);
        return BitConverter.ToInt32(_buffer, _offset + relativeOffset);
    }
    
    public string ReadUtf8String(int length) {
        if (length < 0) {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        if (length == 0) {
            return string.Empty;
        }

        EnsureAvailable(length);
        var result = Encoding.UTF8.GetString(_buffer, _offset, length);

        _offset += length;
        if (_offset == _count) {
            _offset = 0;
            _count = 0;
        }

        return result;
    }

    public void Skip(int bytesToSkip) {
        if (bytesToSkip < 0) {
            throw new ArgumentOutOfRangeException(nameof(bytesToSkip));
        }
        
        if (bytesToSkip == 0) {
            return;
        }

        EnsureAvailable(bytesToSkip);
        _offset += bytesToSkip;
        if (_offset == _count) {
            _offset = 0;
            _count = 0;
        }
    }

    private void EnsureAvailable(int requiredBytes) {
        if (Available >= requiredBytes) {
            return;
        }

        if (_offset > 0) {
            var available = Available;
            if (available > 0) {
                Buffer.BlockCopy(_buffer, _offset, _buffer, 0, available);
            }
            _offset = 0;
            _count = available;
        }

        EnsureCapacity(requiredBytes);
        while (_count < requiredBytes) {
            var read = _stream.Read(_buffer, _count, _buffer.Length - _count);
            if (read == 0) {
                throw new EndOfStreamException();
            }
            _count += read;
            if (_count < requiredBytes && _count == _buffer.Length) {
                EnsureCapacity(requiredBytes);
            }
        }
    }

    private void EnsureCapacity(int requiredBytes) {
        if (_buffer.Length >= requiredBytes) {
            return;
        }

        var newSize = _buffer.Length;
        while (newSize < requiredBytes) {
            newSize *= 2;
        }
        
        Array.Resize(ref _buffer, newSize);
    }
}