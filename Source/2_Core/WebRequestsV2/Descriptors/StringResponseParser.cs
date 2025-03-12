using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public class StringResponseParser : IWebRequestResponseParser<string> {
        public Task<string?> ParseResponse(byte[] bytes) => Task.FromResult(Encoding.UTF8.GetString(bytes))!;
    }
}