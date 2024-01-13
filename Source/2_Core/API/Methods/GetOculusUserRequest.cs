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
        private const string Endpoint = BLConstants.BEATLEADER_API_URL + "/oculususer";

        public static IEnumerator SendRequest(Action<OculusUserInfo> onSuccess, Action<string> onFail) {
            var ticketTask = Authentication.PlatformTicket();
            yield return new WaitUntil(() => ticketTask.IsCompleted);
                
            var authToken = ticketTask.Result;
            if (authToken == null) {
                onFail("Authentication failed");
                yield break;
            }

            var requestDescriptor = new JsonPostRequestDescriptor<OculusUserInfo>(Endpoint, new List<IMultipartFormSection> {
                new MultipartFormDataSection("token", authToken),
            });
            yield return NetworkingUtils.SimpleRequestCoroutine(requestDescriptor, onSuccess, onFail);
        }
    }
}