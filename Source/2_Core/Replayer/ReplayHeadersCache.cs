using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatLeader.Models;

namespace BeatLeader {
    internal static class ReplayHeadersCache {
        #region Cache

        private static readonly AppCache<Dictionary<string, SerializableReplayInfo>> infoCache = new("ReplayInfoCache");

        public static void SaveCache() {
            infoCache.Save();
        }

        public static void LoadCache() {
            infoCache.Load();
        }

        #endregion

        #region Info

        public static bool TryGetInfoByPath(string path, out IReplayInfo? info) {
            if (!infoCache.Cache.TryGetValue(Path.GetFileName(path), out var serInfo)) {
                info = null;
                return false;
            }
            info = serInfo;
            return true;
        }

        public static void AddInfoByPath(string path, IReplayInfo info) {
            infoCache.Cache[Path.GetFileName(path)] = ToSerializableReplayInfo(info);
        }

        public static void RemoveInfoByPath(string path) {
            infoCache.Cache.Remove(Path.GetFileName(path));
        }

        public static void ClearInfo() {
            infoCache.Cache.Clear();
        }

        private static SerializableReplayInfo ToSerializableReplayInfo(IReplayInfo info) {
            return new() {
                FailTime = info.FailTime,
                LevelEndType = info.LevelEndType,
                PlayerID = info.PlayerID,
                PlayerName = info.PlayerName,
                SongDifficulty = info.SongDifficulty,
                SongHash = info.SongHash,
                SongMode = info.SongMode,
                SongName = info.SongName,
                Score = info.Score,
                Timestamp = info.Timestamp
            };
        }

        #endregion
    }
}