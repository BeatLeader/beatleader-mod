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
    public class LeaderboardContextsManager : MonoBehaviour {

        private void Start() {
            ContextsRequest.StateChangedEvent += ContextsRequest_StateChangedEvent;
            ContextsRequest.Send();
        }

        public void ContextsRequest_StateChangedEvent(WebRequests.IWebRequest<List<ServerScoresContext>> instance, WebRequests.RequestState state, string? failReason) {
            if (state == WebRequests.RequestState.Finished) {
                var tasks = new List<Task>();
                ScoresContexts.AllContexts = instance.Result.Select(s => {
                    var context = new ScoresContext {
                        Id = s.Id,
                        Icon = BundleLoader.GeneralContextIcon,
                        Name = s.Name,
                        Description = s.Description,
                        Key = s.Key,
                    };
                    
                    tasks.Add(LoadIconCoroutine(s.Icon, sprite => context.Icon = sprite));
                    
                    return context;
                }).ToList();

                StartCoroutine(WaitImages(tasks));
            } else if (state == WebRequests.RequestState.Failed) {
                Plugin.Log.Debug($"Contexts retrieval failed! {failReason}");
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

            PluginConfig.NotifyScoresContextListWasChanged();
        }
    }
}
