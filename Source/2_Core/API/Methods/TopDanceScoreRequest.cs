using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API;

public class TopDanceScoreRequest : PersistentWebRequestBase<Paged<Score>, JsonResponseParser<Paged<Score>>> {
    public static IWebRequest<Paged<Score>> Send(string player, string hash, string difficulty, string mode) {
        var url = $"{BLConstants.BEATLEADER_API_URL}/v3/scores/{hash}/{difficulty}/{mode}/dnc/global/page?player={player}&page=1&count=1";
        return SendRet(url, HttpMethod.Get);
    }
}
