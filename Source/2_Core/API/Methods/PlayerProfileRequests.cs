using System.Collections.Generic;
using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    internal class PlayerProfileRequest : PersistentWebRequestBase<Player, JsonResponseParser<Player>> {
        public static IWebRequest<Player> Send(string playerId) {
            return SendRet($"{BLConstants.BEATLEADER_API_URL}/player/{playerId}?stats=true", HttpMethod.Get, waitForLogin: false);
        }
    }

    internal class PlayerFollowersInfoRequest : PersistentWebRequestBase<PlayerFollowersInfo, JsonResponseParser<PlayerFollowersInfo>> {
        public static IWebRequest<PlayerFollowersInfo> Send(string playerId) {
            return SendRet($"{BLConstants.BEATLEADER_API_URL}/player/{playerId}/followersInfo", HttpMethod.Get, waitForLogin: false);
        }
    }

    internal class PlayerFollowersRequest : PersistentWebRequestBase<List<PlayerFollower>, JsonResponseParser<List<PlayerFollower>>> {
        public static IWebRequest<List<PlayerFollower>> Send(string playerId, bool following, int page, int count) {
            return SendRet($"{BLConstants.BEATLEADER_API_URL}/player/{playerId}/followers?type={(following ? 0 : 1)}&page={page}&count={count}", HttpMethod.Get, waitForLogin: false);
        }
    }
}
