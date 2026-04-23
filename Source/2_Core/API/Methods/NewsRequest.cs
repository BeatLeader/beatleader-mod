using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.APIV2 {
    public class NewsRequest : PersistentSingletonWebRequestBase<NewsRequest, Paged<NewsPost>, JsonResponseParser<Paged<NewsPost>>> {
        public static void SendRequest() {
            SendRet($"{BLConstants.BEATLEADER_API_URL}/mod/news", HttpMethod.Get);
        }
    }
}