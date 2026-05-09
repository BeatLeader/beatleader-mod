using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatLeader.Models;

namespace BeatLeader {
    internal static class ReplayHeadersCache {
        #region Cache

        private static readonly AppCache<Dictionary<string, SerializableReplayInfo>> infoCache = new("ReplayInfoCache");
        private static readonly object infoCacheLock = new();

        public static void SaveCache() {
            lock (infoCacheLock) {
                infoCache.Save();
            }
        }

        public static void LoadCache() {
            lock (infoCacheLock) {
                infoCache.Load();
            }
        }

        #endregion

        #region Info

        public static bool TryGetInfoByPath(string path, out IReplayInfo? info) {
            lock (infoCacheLock) {
                if (!infoCache.Cache.TryGetValue(Path.GetFileName(path), out var serInfo)) {
                    info = null;
                    return false;
                }

                info = serInfo;
                return true;
            }
        }

        public static void AddInfoByPath(string path, IReplayInfo info) {
            lock (infoCacheLock) {
                infoCache.Cache[Path.GetFileName(path)] = ToSerializableReplayInfo(info);
            }
        }

        public static void RemoveInfoByPath(string path) {
            lock (infoCacheLock) {
                infoCache.Cache.Remove(Path.GetFileName(path));
            }
        }

        public static void ClearInfo() {
            lock (infoCacheLock) {
                infoCache.Cache.Clear();
            }
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
