using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public class RawWebRequestDescriptor : IWebRequestDescriptor<byte[]> {
        public Task<byte[]?> ParseResponse(byte[] bytes) => Task.FromResult(bytes)!;
    }
}