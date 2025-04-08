using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    public class TrendingMapsRequest : PersistentSingletonRequestHandler<TrendingMapsRequest, Paged<TrendingMapData>> {
        public static void SendRequest() {
            var descriptor = new JsonGetRequestDescriptor<Paged<TrendingMapData>>($"{BLConstants.BEATLEADER_API_URL}/mod/maps/trending");
            Instance.Send(descriptor);
        }
    }
}