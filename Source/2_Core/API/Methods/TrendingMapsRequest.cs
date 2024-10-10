using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    public class TrendingMapsRequest : PersistentSingletonRequestHandler<TrendingMapsRequest, Paged<MapData>> {
        public static void SendRequest() {
            var descriptor = new JsonGetRequestDescriptor<Paged<MapData>>($"{BLConstants.BEATLEADER_API_URL}/maps/trending");
            Instance.Send(descriptor);
        }
    }
}