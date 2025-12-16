using System.Collections.Generic;
using System.Net.Http;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;

namespace BeatLeader.API {
    public class PrestigeLevelsRequest : PersistentSingletonWebRequestBase<PrestigeLevelsRequest, List<PrestigeLevel>, JsonResponseParser<List<PrestigeLevel>>> {
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/experience/levels";

        public static void Send() {
            SendRet(Endpoint, HttpMethod.Get);
        }
    }
}

