using System.Collections.Generic;

namespace BeatLeader;

public class BufferPool {
    private static readonly List<byte[]> buffers = new();
    private static readonly object locker = new();

    // TODO: implement binary sorting
    public static byte[] Borrow(int size) {
        lock (locker) {
            for (var i = 0; i < buffers.Count; i++) {
                var buffer = buffers[i];

                if (buffer.Length >= size) {
                    buffers.Remove(buffer);
                    return buffer;
                }
            }
        }

        return new byte[size];
    }

    public static void Release(byte[] buffer) {
        lock (locker) {
            buffers.Add(buffer);
        }
    }
}