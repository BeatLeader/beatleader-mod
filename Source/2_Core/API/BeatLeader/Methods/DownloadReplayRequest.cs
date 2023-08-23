using System;
using BeatLeader.API.RequestHandlers;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using UnityEngine.Networking;

namespace BeatLeader.API.Methods {
    internal class DownloadReplayRequest : PersistentSingletonRequestHandler<DownloadReplayRequest, Replay> {
        protected override bool KeepState => false;

        public static void SendRequest(string replayUrl) {
            var requestDescriptor = new RequestDescriptor(replayUrl);
            instance.Send(requestDescriptor);
        }

        private class RequestDescriptor : IWebRequestDescriptor<Replay> {
            private readonly string _url;

            public RequestDescriptor(string url) {
                _url = url;
            }

            public UnityWebRequest CreateWebRequest() {
                return UnityWebRequest.Get(_url);
            }

            public Replay ParseResponse(UnityWebRequest request) {
                if (!ReplayDecoder.TryDecodeReplay(request.downloadHandler.data, out var replay)) {
                    throw new Exception("Unable to decode replay!");
                }

                return replay;
            }
        }
    }
}