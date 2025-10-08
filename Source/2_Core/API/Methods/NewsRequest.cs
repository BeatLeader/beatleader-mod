using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    public class NewsRequest : PersistentSingletonRequestHandler<NewsRequest, Paged<NewsPost>> {
        public static void SendRequest() {
            var descriptor = new JsonGetRequestDescriptor<Paged<NewsPost>>($"{BLConstants.BEATLEADER_API_URL}/mod/news");
            Instance.Send(descriptor);
        }
    }
}