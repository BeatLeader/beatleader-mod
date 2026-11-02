using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Utils;
using Reactive.Components.GIF;
using UnityEngine;

namespace BeatLeader.DataManager {
    public static class LeaderboardContextsManager {
        public static void Initialize() {
            ContextsRequest.Send().StateChangedEvent += HandleStateChanged;
        }

        private static void HandleStateChanged(WebRequests.IWebRequest<ServerScoresContext[]> instance, WebRequests.RequestState state, string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Finished: 
                    var tasks = new List<Task>(instance.Result!.Length);

                    ScoresContexts.allContexts = instance.Result.Select(s => {
                        var context = new ScoresContext {
                            Id = s.Id,
                            Icon = BundleLoader.GeneralContextIcon,
                            Name = s.Name,
                            Description = s.Description,
                            Key = s.Key,
                        };

                        tasks.Add(LoadIconAsync(s.Icon, context));

                        return context;
                    }).ToArray();

                    _ = Task.WhenAll(tasks)
                        .ContinueWith(_ => PluginConfig.NotifyScoresContextListWasChanged())
                        .RunCatching();

                    break;

                case WebRequests.RequestState.Failed:
                    Plugin.Log.Debug($"Contexts retrieval failed! {failReason}");
                    break;
            }
        }

        private static async Task LoadIconAsync(string url, ScoresContext context) {
            var image = await ImageLoader.LoadImage(url, CancellationToken.None);

            if (image != null) {
                context.Icon = image.Sprite ?? GetFirstFrame(image);
            } else {
                Plugin.Log.Error("Failed to load context icon");
            }
        }

        private static Sprite GetFirstFrame(CachedImage image) {
            var player = new GifPlayer { Image = image };
            player.Stop();
            return player.Sprite!;
        }
    }
}