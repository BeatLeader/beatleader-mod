using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.APIV2;
using BeatLeader.Models;
using BeatLeader.Utils;
using BGLib.UnityExtension;
using Reactive;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader.DataManager {
    public static class PrestigeLevelsManager {
        #region Loading

        private record struct PrestigeRequest(
            UnityWebRequest Request,
            int Level
        );

        private static bool _initialized;
        private static bool _loading;

        private static readonly ConcurrentQueue<PrestigeRequest> _requestsQueue = new();
        private static SynchronizationContext? _mainThreadSynchronizationContext;
        private static float _lastSyncTime;
        private static int _processedLevels;
        private static int _totalLevels;

        public static void Initialize() {
            if (_initialized || _loading) return;

            if (Thread.CurrentThread.ManagedThreadId != 1) {
                throw new InvalidOperationException("Initialize must be called from the main thread");
            }

            _mainThreadSynchronizationContext = SynchronizationContext.Current;
            _loading = true;

            PrestigeLevelsRequest.StateChangedEvent += OnPrestigeLevelsRequestStateChanged;
            PrestigeLevelsRequest.Send();
        }

        private static void OnPrestigeLevelsRequestStateChanged(
            WebRequests.IWebRequest<List<PrestigeLevel>> instance,
            WebRequests.RequestState state,
            string failReason
        ) {
            switch (state) {
                case WebRequests.RequestState.Finished:
                    var levels = instance.Result!;

                    foreach (var level in levels) {
                        if (string.IsNullOrEmpty(level.BigIcon)) {
                            continue;
                        }
                        
                        _totalLevels++;
                        _ = LoadIconAsync(level.BigIcon, level.Level).RunCatching();
                    }
                    break;

                case WebRequests.RequestState.Failed:
                    Plugin.Log.Debug($"Prestige levels retrieval failed! {failReason}");
                    break;
            }

            _loading = false;
        }

        private static async Task LoadIconAsync(string url, int level) {
            var request = UnityWebRequestTexture.GetTexture(url);
            var result = await request.SendWebRequestAsync();

            if (result != UnityWebRequest.Result.Success) {
                Plugin.Log.Debug($"Failed to load prestige icon from {url}: {request.error}");
                _processedLevels++;
                return;
            }

            var req = new PrestigeRequest(request, level);
            _requestsQueue.Enqueue(req);

            EnqueueSpriteJob();
        }

        private static void EnqueueSpriteJob() {
            _mainThreadSynchronizationContext!.Post(static _ => {
                var time = Time.time;

                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (_lastSyncTime == time) {
                    return;
                }

                _lastSyncTime = time;

                if (_requestsQueue.TryDequeue(out var request)) {
                    try {
                        var texture = DownloadHandlerTexture.GetContent(request.Request);
                        var sprite = SpriteUtils.CreateSprite(texture);

                        _icons[request.Level] = sprite!;
                    } finally {
                        request.Request.Dispose();
                        _processedLevels++;
                    }
                }

                if (!_requestsQueue.IsEmpty) {
                    EnqueueSpriteJob();
                    return;
                }

                if (_processedLevels == _totalLevels) {
                    _initialized = true;
                    _loading = false;

                    _iconsLoaded?.Invoke();
                }
            }, null);
        }

        #endregion

        #region Public API

        public static event Action? IconsLoadedEvent {
            add {
                if (_initialized) {
                    value?.Invoke();
                }
                _iconsLoaded += value;
            }
            remove => _iconsLoaded -= value;
        }

        private static Action? _iconsLoaded;
        private static readonly Dictionary<int, Sprite> _icons = new();

        public static Sprite GetIcon(int level) {
            if (_icons.TryGetValue(level, out var sprite)) {
                return sprite;
            }
            return BundleLoader.TransparentPixel;
        }

        public static bool HasIcon(int level) {
            return _icons.ContainsKey(level);
        }

        #endregion
    }
}