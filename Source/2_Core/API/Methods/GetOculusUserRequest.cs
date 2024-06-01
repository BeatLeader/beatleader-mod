using System;
using System.Collections;
using System.Collections.Generic;
using BeatLeader.API.RequestDescriptors;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal static class GetOculusUserRequest {
        // /oculususer
        private static string Endpoint => BLConstants.BEATLEADER_API_URL + "/oculususer";

        public static IEnumerator SendRequest(Action<OculusUserInfo> onSuccess, Action<string> onFail) {
            var ticketTask = Authentication.PlatformTicket();
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