using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    public class EarthDayMap
    {
        public int id { get; set; }
        public string hash { get; set; }
        public int timeset { get; set; }
        public string playerId { get; set; }
    }

    public class EarthDayRequest : PersistentSingletonRequestHandler<EarthDayRequest, EarthDayMap> {
        public static void SendRequest(string playerId) {
            var descriptor = new JsonGetRequestDescriptor<EarthDayMap>($"{BLConstants.BEATLEADER_API_URL}/earthday/map/{playerId}");
            Instance.Send(descriptor);
        }
    }
}