using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BeatLeader.Models;

namespace BeatLeader {
    internal static class ReplayHeadersCache {
        #region Cache

        private static readonly AppCache<Dictionary<string, SerializableReplayInfo?>> infoCache = new("ReplayInfoCache");
        private static readonly object locker = new();

        public static Task WaitForLoading() {
            // ReSharper disable once InconsistentlySynchronizedField
            return infoCache.WaitForLoading();
        }

        public static void SaveCache() {
            lock (locker) {
                infoCache.SaveDetached();
            }
        }

        public static void LoadCache() {
            lock (locker) {
                infoCache.LoadDetached();
            }
        }

        #endregion

        #region Info

        public static bool TryGetInfoByPath(string path, out IReplayInfo? info) {
            lock (locker) {
                if (!infoCache.Cache.TryGetValue(Path.GetFileName(path), out var serInfo)) {
                    info = null;
                    return false;
                }

                info = serInfo;
            }

            return true;
        }

        public static void AddInfoByPath(string path, IReplayInfo? info) {
            lock (locker) {
                infoCache.Cache[Path.GetFileName(path)] = info == null ? null : ToSerializableReplayInfo(info);
            }
        }

        public static void RemoveInfoByPath(string path) {
            lock (locker) {
                infoCache.Cache.Remove(Path.GetFileName(path));
            }
        }

        public static void ClearInfo() {
            lock (locker) {
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