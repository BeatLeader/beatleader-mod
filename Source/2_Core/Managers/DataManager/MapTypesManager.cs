using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.APIV2;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using Reactive.Components;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal static class MapTypesManager {
        public static IReadOnlyList<MapsTypeDescription> MapTypes => _mapTypes;

        private static MapsTypeDescription[] _mapTypes = Array.Empty<MapsTypeDescription>();
        private static Dictionary<MapsTypeDescription, Sprite> _sprites = new();

        public static void Initialize() {
            MapTypesRequest.Send().StateChangedEvent += HandleStateChanged;
        }

        public static Sprite GetIcon(MapsTypeDescription description) {
            return _sprites[description];
        }

        private static void HandleStateChanged(IWebRequest<MapsTypeDescription[]> instance, RequestState state, string? failReason) {
            switch (state) {
                case RequestState.Finished: {
                    _mapTypes = instance.Result!;
                    
                    foreach (var description in _mapTypes) {
                        _ = LoadIconAsync(description).RunCatching();
                    }
                    break;
                }
                
                case RequestState.Failed:
                    Plugin.Log.Debug($"Map types retrieval failed: {failReason}");
                    break;
            }
        }

        private static async Task LoadIconAsync(MapsTypeDescription description) {
            var image = await ImageLoader.LoadImage(description.Icon, CancellationToken.None);

            if (image != null) {
                _sprites[description] = image.Sprite;
            } else {
                Plugin.Log.Error("Failed to load map type icon");
            }
        }
    }
}