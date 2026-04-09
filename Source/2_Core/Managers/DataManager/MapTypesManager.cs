using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using IPA.Utilities.Async;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader.DataManager {
    public class MapTypesManager : MonoBehaviour {

        public static List<MapsTypeDescription>? MapsTypes = null;

        private void Start() {
            MapTypesRequest.StateChangedEvent += MapTypesRequest_StateChangedEvent;
            MapTypesRequest.Send();
        }

        private void OnDestroy() {
            MapTypesRequest.StateChangedEvent -= MapTypesRequest_StateChangedEvent;
        }

        public void MapTypesRequest_StateChangedEvent(WebRequests.IWebRequest<List<MapsTypeDescription>> instance, WebRequests.RequestState state, string? failReason) {
            if (state == WebRequests.RequestState.Finished) {
                var tasks = new List<Task>();
                MapsTypes = instance.Result.Select(s => {
                    
                    tasks.Add(LoadIconCoroutine(s.Icon, sprite => s.Sprite = sprite));
                    
                    return s;
                }).ToList();

                StartCoroutine(WaitImages(tasks));
            } else if (state == WebRequests.RequestState.Failed) {
                Plugin.Log.Debug($"Map types retrieval failed! {failReason}");
            }
        }
        
        private static async Task LoadIconCoroutine(string url, Action<Sprite> onLoaded) {
            var request = await RawDataRequest.Send(url).Join();
            
            if (request.RequestState != WebRequests.RequestState.Finished) {
                Plugin.Log.Debug($"Failed to load icon from {url}: {request.FailReason}");
                return;
            }
                
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
            var loaded = texture.LoadImage(request.Result);
            if (loaded) {
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                    new Vector2(0.5f, 0.5f));
                onLoaded(sprite);
            }
        }

        private IEnumerator WaitImages(List<Task> tasks) {
            foreach (var task in tasks) {
                yield return task;
            }
        }
    }
}
