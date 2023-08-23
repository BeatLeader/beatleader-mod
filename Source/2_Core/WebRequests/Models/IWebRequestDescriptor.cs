using System.Threading.Tasks;

namespace BeatLeader.WebRequests {
    public interface IWebRequestDescriptor<T> {
        Task<T?> ParseResponse(byte[] bytes);
    }
}