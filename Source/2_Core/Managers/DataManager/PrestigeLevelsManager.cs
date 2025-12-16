using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.DataManager {
    public static class PrestigeLevelsManager {
        private static readonly Dictionary<int, Sprite> BigIcons = new();
        private static readonly Dictionary<int, Sprite> SmallIcons = new();
        private static bool _initialized;
        private static bool _loading;

        public static event Action IconsLoadedEvent;

        public static void Initialize() {
            if (_initialized || _loading) return;
            _loading = true;
            
            PrestigeLevelsRequest.StateChangedEvent += OnPrestigeLevelsRequestStateChanged;
            PrestigeLevelsRequest.Send();
        }

        private static void OnPrestigeLevelsRequestStateChanged(
            WebRequests.IWebRequest<List<PrestigeLevel>> instance,
            WebRequests.RequestState state,
            string failReason
        ) {
            if (state == WebRequests.RequestState.Finished) {
                LoadAllIcons(instance.Result);
            } else if (state == WebRequests.RequestState.Failed) {
                Plugin.Log.Debug($"Prestige levels retrieval failed! {failReason}");
                _loading = false;
            }
        }

        private static async void LoadAllIcons(List<PrestigeLevel> levels) {
            var tasks = new List<Task>();

            foreach (var level in levels) {
                if (!string.IsNullOrEmpty(level.BigIcon)) {
                    tasks.Add(LoadIconAsync(level.BigIcon, sprite => BigIcons[level.Level] = sprite));
                }
                if (!string.IsNullOrEmpty(level.SmallIcon)) {
                    tasks.Add(LoadIconAsync(level.SmallIcon, sprite => SmallIcons[level.Level] = sprite));
                }
            }

            await Task.WhenAll(tasks);
            
            _initialized = true;
            _loading = false;
            
            IconsLoadedEvent?.Invoke();
        }

        private static async Task LoadIconAsync(string url, Action<Sprite> onLoaded) {
            try {
                var request = await RawDataRequest.Send(url).Join();

                if (request.RequestState != WebRequests.RequestState.Finished) {
                    Plugin.Log.Debug($"Failed to load prestige icon from {url}: {request.FailReason}");
                    return;
                }

                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                var loaded = texture.LoadImage(request.Result);
                if (loaded) {
                    var sprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f)
                    );
                    onLoaded(sprite);
                }
            } catch (Exception ex) {
                Plugin.Log.Debug($"Exception loading prestige icon from {url}: {ex.Message}");
            }
        }

        public static Sprite GetBigIcon(int level) {
            if (BigIcons.TryGetValue(level, out var sprite)) {
                return sprite;
            }
            return BundleLoader.TransparentPixel;
        }

        public static Sprite GetSmallIcon(int level) {
            if (SmallIcons.TryGetValue(level, out var sprite)) {
                return sprite;
            }
            return BundleLoader.TransparentPixel;
        }

        public static bool HasIcon(int level) {
            return BigIcons.ContainsKey(level) || SmallIcons.ContainsKey(level);
        }
    }
}

