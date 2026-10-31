using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using Newtonsoft.Json;

namespace BeatLeader {
    internal static class DanceEvent {
        public const int EventType = 2;

        private const int HashLength = 40;

        public static PlatformEvent? Current { get; private set; }

        public static event Action? CurrentChangedEvent;

        public static bool IsEventLevel(BeatmapKey key) {
            if (Current is not { } evt || !evt.IsHappening()) return false;
            var levelId = key.levelId;
            var prefix = CustomLevelLoader.kCustomLevelPrefixId;
            if (!levelId.StartsWith(prefix) || levelId.Length < prefix.Length + HashLength) return false;
            var hash = levelId.Substring(prefix.Length, HashLength);
            return _maps.Contains(hash) || _maps.Contains(MapKey(hash, key.beatmapCharacteristic.serializedName, key.difficulty.ToString()));
        }

        #region Events

        private static bool _loading;

        public static void Load() {
            if (_loading || Current != null) return;
            LoadEvents().RunCatching();
        }

        private static async Task LoadEvents() {
            _loading = true;
            try {
                var request = await PlatformEventsRequest.Send().Join();
                if (request.RequestState == RequestState.Finished) SetEvents(request.Result?.data);
            } finally {
                _loading = false;
            }
        }

        public static void SetEvents(IReadOnlyList<PlatformEvent>? events) {
            if (events == null) return;
            var current = events.FirstOrDefault(x => x.eventType == EventType && x.IsHappening());
            var changed = current?.id != Current?.id;
            Current = current;
            if (!changed) return;
            _maps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (current != null) LoadMaps(current).RunCatching();
            CurrentChangedEvent?.Invoke();
        }

        #endregion

        #region Maps

        private static HashSet<string> _maps = new(StringComparer.OrdinalIgnoreCase);

        private static string MapKey(string hash, string characteristic, string difficulty) {
            return $"{hash}|{characteristic}|{difficulty}";
        }

        private static async Task LoadMaps(PlatformEvent evt) {
            var request = await PlaylistRequest.Send(evt.playlistId.ToString()).Join();
            if (request.RequestState != RequestState.Finished || request.Result == null) {
                Plugin.Log.Warn($"Couldn't load the maps of {evt.name}: {request.FailReason}");
                return;
            }
            if (Current?.id != evt.id) return;
            _maps = ParseMaps(request.Result);
            Plugin.Log.Info($"{evt.name}: {_maps.Count} event difficulties");
        }

        private static HashSet<string> ParseMaps(byte[] bytes) {
            var maps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var playlist = JsonConvert.DeserializeObject<Playlist>(Encoding.UTF8.GetString(bytes).TrimStart('\uFEFF'));
            foreach (var song in playlist?.songs ?? new()) {
                if (song.hash is not { Length: >= HashLength } hash) continue;
                hash = hash.Substring(0, HashLength);
                if (song.difficulties is not { Count: > 0 } difficulties) {
                    maps.Add(hash);
                    continue;
                }
                foreach (var difficulty in difficulties) {
                    if (difficulty.name == null) continue;
                    maps.Add(MapKey(hash, difficulty.characteristic ?? "Standard", difficulty.name));
                }
            }
            return maps;
        }

        private class Playlist {
            public List<PlaylistSong>? songs;
        }

        private class PlaylistSong {
            public string? hash;
            public List<PlaylistDifficulty>? difficulties;
        }

        private class PlaylistDifficulty {
            public string? name;
            public string? characteristic;
        }

        #endregion
    }
}
