using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public class StringWebRequestDescriptor : IWebRequestDescriptor<string> {
        public Task<string?> ParseResponse(byte[] bytes) => Task.FromResult(Encoding.UTF8.GetString(bytes))!;
    }
}