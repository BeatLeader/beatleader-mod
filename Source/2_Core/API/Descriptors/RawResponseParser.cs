using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public class RawResponseParser : IWebRequestResponseParser<byte[]> {
        public Task<byte[]?> ParseResponse(byte[] bytes) => Task.FromResult(bytes)!;
    }
}