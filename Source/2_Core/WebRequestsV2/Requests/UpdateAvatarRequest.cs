using System.Net.Http;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class UpdateAvatarRequest : PersistentWebRequestBase {
        public static IWebRequest Send(string playerId) {
            return Send($"{BLConstants.BEATLEADER_API_URL}/player/{playerId}/ingameavatar", HttpMethod.Post);
        }
    }
}