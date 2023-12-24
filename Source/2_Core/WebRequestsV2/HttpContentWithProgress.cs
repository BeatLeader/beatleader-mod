using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.WebRequests {
    public class HttpContentWithProgress : HttpContent {
        private readonly HttpContent _content;
        private readonly IIoOperationDescriptor _descriptor;

        public HttpContentWithProgress(
            HttpContent content,
            IIoOperationDescriptor descriptor
        ) {
            _content = content;
            _descriptor = descriptor;
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context) {
            using var inputStream = await _content.ReadAsStreamAsync();
            var contentLength = _content.Headers.ContentLength!.Value;
            _descriptor.ContentSize = contentLength;
            await StreamUtils.CopyToByBufferAsync(inputStream, stream, _descriptor);
        }

        protected override bool TryComputeLength(out long length) {
            length = _content.Headers.ContentLength.GetValueOrDefault();
            return true;
        }
    }
}