using System.Net.Http;
using System.Text;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using Newtonsoft.Json;

namespace BeatLeader.APIV2 {
    internal class UpdateAvatarRequest : PersistentWebRequestBase {
        public static IWebRequest<object> Send(string playerId, AvatarSettings avatarSettings) {
            var body = JsonConvert.SerializeObject(avatarSettings);
            return Send($"{BLConstants.BEATLEADER_API_URL}/player/{playerId}/ingameavatar", HttpMethod.Post,
                () => new StringContent(body, Encoding.UTF8, "application/json"));
        }
    }
}