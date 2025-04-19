using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
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
                var tasks = new List<IEnumerator>();
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
        
        private static IEnumerator LoadIconCoroutine(string url, Action<Sprite> onLoaded) {
            using var www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success) {
                Plugin.Log.Debug($"Failed to load icon from {url}: {www.error}");
                yield break;
            }
            
            var texture = DownloadHandlerTexture.GetContent(www);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f));
            onLoaded(sprite);
        }

        private IEnumerator WaitImages(List<IEnumerator> tasks) {
            foreach (var task in tasks) {
                yield return task;
            }

            PluginConfig.NotifyScoresContextListWasChanged();
        }
    }
}
