using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public class RawResponseParser : IWebRequestResponseParser<byte[]> {
        public byte[]? ParseResponse(byte[] bytes) => bytes;
    }
}