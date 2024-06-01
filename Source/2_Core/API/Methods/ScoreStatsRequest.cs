﻿using BeatLeader.API.RequestDescriptors;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal class ScoreStatsRequest : PersistentSingletonRequestHandler<ScoreStatsRequest, ScoreStats> {
        // score/statistic/{scoreId}
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/score/statistic/{0}";

        public static void SendRequest(int scoreId) {
            var url = string.Format(Endpoint, scoreId);
            var requestDescriptor = new JsonGetRequestDescriptor<ScoreStats>(url);
            Instance.Send(requestDescriptor);
        }
    }
}