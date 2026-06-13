using System;
using System.Runtime.InteropServices;
using System.Text;

namespace BeatLeader;

public class UnmanagedFileReader : IDisposable {
    #region Init

    private readonly int _bufferSize;
    private readonly IntPtr _fileHandle;
    private IntPtr _bufferPtr;

    private int _currentBufferSize;
    private int _offset;
    private bool _readWhole;

    public UnmanagedFileReader(string path, int bufferSize) {
        _fileHandle = CreateFileW(
            path,
            GENERIC_READ,
            FILE_SHARE_READ,
            IntPtr.Zero,
            OPEN_EXISTING,
            0,
            IntPtr.Zero
        );

        if (_fileHandle == invalidHandleValue) {
            throw GetWin32Exception();
        }

        _bufferPtr = Marshal.AllocHGlobal(bufferSize);
        _bufferSize = bufferSize;
        _currentBufferSize = bufferSize;

        ReadToBuffer(0, bufferSize);
    }

    public void Dispose() {
        CloseHandle(_fileHandle);
        Marshal.FreeHGlobal(_bufferPtr);
    }

    private static Exception GetWin32Exception() {
        return new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    }

    #endregion

    #region Read

    public byte ReadByte() {
        EnsureBufferAvailable(sizeof(byte));
        unsafe {
            var ptr = (byte*)(_bufferPtr + _offset);

            _offset += sizeof(byte);

            return *ptr;
        }
    }

    public int ReadInt32() {
        EnsureBufferAvailable(sizeof(int));
        unsafe {
            var ptr = (int*)(_bufferPtr + _offset);

            _offset += sizeof(int);

            return *ptr;
        }
    }

    public float ReadSingle() {
        EnsureBufferAvailable(sizeof(float));
        unsafe {
            var ptr = (float*)(_bufferPtr + _offset);

            _offset += sizeof(float);

            return *ptr;
        }
    }

    public bool ReadBool() {
        EnsureBufferAvailable(sizeof(byte));
        unsafe {
            var ptr = (bool*)(_bufferPtr + _offset);

            _offset += sizeof(bool);

            return *ptr;
        }
    }

    public string ReadUtf8String(int length) {
        if (length == 0) {
            return string.Empty;
        }

        EnsureBufferAvailable(length);

        unsafe {
            var ptr = (byte*)(_bufferPtr + _offset);
            var result = Encoding.UTF8.GetString(ptr, length);

            _offset += length;

            return result;
        }
    }

    #endregion

    #region Helpers

    public int PeekInt32(int relativeOffset) {
        EnsureBufferAvailable(relativeOffset + sizeof(int));

        unsafe {
            return *(int*)(_bufferPtr + _offset + relativeOffset);
        }
    }

    public void Skip(int bytes) {
        EnsureBufferAvailable(bytes);
        _offset += bytes;
    }

    #endregion

    #region Buffer

    public void EnsureBufferAvailable(int neededBytes) {
        if (_readWhole || _offset + neededBytes <= _currentBufferSize - 1) {
            return;
        }

        var bufferOffset = _currentBufferSize;

        _currentBufferSize += _bufferSize;
        _bufferPtr = Marshal.ReAllocHGlobal(_bufferPtr, (IntPtr)_currentBufferSize);

        ReadToBuffer(bufferOffset, _bufferSize);
    }

    private void ReadToBuffer(int bufferOffset, int readCount) {
        var bufferPtr = _bufferPtr + bufferOffset;

        if (!ReadFile(_fileHandle, bufferPtr, (uint)readCount, out var bytesRead, IntPtr.Zero)) {
            throw GetWin32Exception();
        }

        if (bytesRead < readCount) {
            _readWhole = true;
        }
    }

    #endregion

    #region Native

    private const uint GENERIC_READ = 0x80000000;
    private const uint FILE_SHARE_READ = 0x00000001;
    private const uint OPEN_EXISTING = 3;

    private static readonly IntPtr invalidHandleValue = new(-1);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateFileW(
        string lpFileName,
        uint dwDesiredAccess,
        uint dwShareMode,
        IntPtr lpSecurityAttributes,
        uint dwCreationDisposition,
        uint dwFlagsAndAttributes,
        IntPtr hTemplateFile
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool ReadFile(
        IntPtr hFile,
        IntPtr lpBuffer,
        uint nNumberOfBytesToRead,
        out uint lpNumberOfBytesRead,
        IntPtr lpOverlapped
    );

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr hObject);

    #endregion
}