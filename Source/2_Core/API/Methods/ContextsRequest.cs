using System;
using System.Collections;
using System.Collections.Generic;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    public class ServerScoresContext {
        public int Id { get; set; }
        public string Icon { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
    }

    internal static class ContextsRequest {

        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/mod/leaderboardContexts";

        public static IEnumerator SendRequest(
            Action<List<ServerScoresContext>> onSuccess,
            Action<string> onFail
        ) {
            var requestDescriptor = new JsonGetRequestDescriptor<List<ServerScoresContext>>(Endpoint);
            yield return NetworkingUtils.SimpleRequestCoroutine(requestDescriptor, onSuccess, onFail);
        }
    }
}