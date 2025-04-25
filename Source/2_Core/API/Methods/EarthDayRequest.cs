using System.Threading.Tasks;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    public class EarthDayMap {
        public int id { get; set; }
        public string hash { get; set; }
        public int timeset { get; set; }
        public string playerId { get; set; }
    }

    public class EarthDayRequest : PersistentSingletonRequestHandler<EarthDayRequest, EarthDayMap> {
        public static void SendMapRequest(string playerId) {
            var url = GetMapUrl(playerId);
            var descriptor = new JsonGetRequestDescriptor<EarthDayMap>(url);
            Instance.Send(descriptor);
        }

        public static Task<EarthDayMap?> SendMapRequestAsync(string playerId) {
            var url = GetMapUrl(playerId);
            return WebUtils.SendAndDeserializeAsync<EarthDayMap>(url);
        }

        public static Task<byte[]?> SendDownloadRequestAsync(EarthDayMap map) {
            var url = GetDownloadUrl(map);
            return WebUtils.SendRawDataRequestAsync(url);
        } 

        private static string GetDownloadUrl(EarthDayMap earthDayMap) {
            return $"https://cdn.songs.beatleader.com/EarthDayMap_{earthDayMap.playerId}_{earthDayMap.timeset}.zip";
        }

        private static string GetMapUrl(string playerId) {
            return $"{BLConstants.BEATLEADER_API_URL}/earthday/map/{playerId}";
        }
    }
}