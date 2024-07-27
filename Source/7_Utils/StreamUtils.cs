using System.IO;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;

namespace BeatLeader.Utils {
    public static class StreamUtils {
        public static async Task CopyToByBufferAsync(
            Stream stream,
            Stream targetStream,
            IIoOperationDescriptor ioOperationDescriptor,
            CancellationToken cancellationToken = default
        ) {
            var buffer = ioOperationDescriptor.Buffer;
            var totalBytesRead = 0L;
            var totalBytes = stream.Length;
            var bytesRead = 0;
            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0) {
                await targetStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
                totalBytesRead += bytesRead;
                ioOperationDescriptor.OnProgressChanged(totalBytesRead, totalBytes);
                if (cancellationToken.IsCancellationRequested) return;
            }
        }
    }
}