using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public class StringResponseParser : IWebRequestResponseParser<string> {
        public string? ParseResponse(byte[] bytes) => Encoding.UTF8.GetString(bytes);
    }
}