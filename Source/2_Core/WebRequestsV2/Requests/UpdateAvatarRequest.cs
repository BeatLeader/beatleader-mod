using System.Net.Http;
using System.Text;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using Newtonsoft.Json;

namespace BeatLeader.API {
    public class UpdateAvatarRequest : PersistentWebRequestBase {
        public static IWebRequest Send(string playerId, AvatarSettings avatarSettings) {
            var body = JsonConvert.SerializeObject(avatarSettings);
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            return Send($"{BLConstants.BEATLEADER_API_URL}/player/{playerId}/ingameavatar", HttpMethod.Post, content);
        }
    }
}