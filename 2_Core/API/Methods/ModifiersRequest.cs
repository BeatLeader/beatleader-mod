using System;
using System.Collections;
using System.Collections.Generic;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.Utils;

namespace BeatLeader.API.Methods {
    internal static class ModifiersRequest {
        // /modifiers
        private const string Endpoint = BLConstants.BEATLEADER_API_URL + "/modifiers";

        public static IEnumerator SendRequest(Action<Dictionary<string, float>> onSuccess, Action<string> onFail) {
            var requestDescriptor = new JsonGetRequestDescriptor<Dictionary<string, float>>(Endpoint);
            yield return NetworkingUtils.SimpleRequestCoroutine(requestDescriptor, onSuccess, onFail, 3);
        }
    }
}