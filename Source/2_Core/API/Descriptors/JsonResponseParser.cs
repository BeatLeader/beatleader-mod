using System.Text;
using System.Threading.Tasks;
using BeatLeader.API;
using Newtonsoft.Json;

namespace BeatLeader.WebRequests {
    public class JsonResponseParser<T> : IWebRequestResponseParser<T> {
        public virtual T? ParseResponse(byte[] bytes) {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(bytes), NetworkingUtils.SerializerSettings);
        }
    }
}