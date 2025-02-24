using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader.DataManager {
    public class LeaderboardContextsManager : MonoBehaviour {

        private void Start() {
            StartCoroutine(UpdateContextsTask());
        }

        private static IEnumerator UpdateContextsTask() {
            var tasks = new List<IEnumerator>();
            void OnSuccess(List<ServerScoresContext> result) {
                ScoresContexts.AllContexts = result.Select(s => {
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
            }

            void OnFail(string reason) {
                Plugin.Log.Debug($"Contexts retrieval failed! {reason}");
            }

            yield return ContextsRequest.SendRequest(OnSuccess, OnFail);
            foreach (var task in tasks) {
                yield return task;
            }

            PluginConfig.NotifyScoresContextListWasChanged();
        }
        
        private static IEnumerator LoadIconCoroutine(string url, Action<Sprite> onLoaded) {
            using var www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();
            
            if (www.responseCode != 200) {
                Plugin.Log.Debug($"Failed to load icon from {url}: {www.error}");
                yield break;
            }
            
            var texture = DownloadHandlerTexture.GetContent(www);
            var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
                new Vector2(0.5f, 0.5f));
            onLoaded(sprite);
        }
    }
}
