using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public interface IWebRequestResponseParser<T> {
        T? ParseResponse(byte[] bytes);
    }
}