using System;
using System.Collections;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.API.Methods {
    internal static class GetOculusUserRequest {
        // /oculususer?token={authToken}
        private const string Endpoint = BeatLeaderConstants.BEATLEADER_API_URL + "/oculususer?token={0}";

        public static IEnumerator SendRequest(Action<OculusUserInfo> onSuccess, Action<string> onFail) {
            var ticketTask = Authentication.OculusTicket();
            yield return new WaitUntil(() => ticketTask.IsCompleted);
                
            var authToken = ticketTask.Result;
            if (authToken == null) {
                onFail("Authentication failed");
                yield break;
            }

            var url = string.Format(Endpoint, authToken);
            var requestDescriptor = new JsonGetRequestDescriptor<OculusUserInfo>(url);
            yield return NetworkingUtils.SimpleRequestCoroutine(requestDescriptor, onSuccess, onFail);
        }
    }
}