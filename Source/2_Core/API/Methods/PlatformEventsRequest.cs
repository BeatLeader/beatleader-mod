using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    public class PlatformEventsRequest : PersistentSingletonRequestHandler<PlatformEventsRequest, Paged<PlatformEvent>> {
        public static void SendRequest() {
            var descriptor = new JsonGetRequestDescriptor<Paged<PlatformEvent>>($"{BLConstants.BEATLEADER_API_URL}/events");
            Instance.Send(descriptor);
        }
    }
}