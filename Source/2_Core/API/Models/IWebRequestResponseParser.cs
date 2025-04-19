using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public interface IWebRequestResponseParser<T> {
        Task<T?> ParseResponse(byte[] bytes);
    }
}