using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BeatLeader.WebRequests {
    public class JsonResponseParser<T> : IWebRequestResponseParser<T> {
        public Task<T?> ParseResponse(byte[] bytes) {
            return Task.FromResult(JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes)))!;
        }
    }
}