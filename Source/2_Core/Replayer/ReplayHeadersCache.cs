using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatLeader.Models;

namespace BeatLeader {
    //TODO: create a storage namespace and put it there with configs & caches
    internal static class ReplayHeadersCache {
        #region Cache

        private static readonly AppCache<Dictionary<string, SerializableReplayInfo>> infoCache = new("ReplayInfoCache");
        private static readonly AppCache<Dictionary<string, SerializableReplayMetadata>> metaCache = new("ReplayMetadataCache");
        private static readonly AppCache<Dictionary<string, SerializableReplayTag>> tagsCache = new("ReplayTagsCache");

        public static void SaveCache() {
            infoCache.Save();
            metaCache.Save();
            tagsCache.Save();
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
                Timestamp = info.Timestamp
            };
        }

        #endregion

        #region Tags & Meta

        public static IDictionary<string, SerializableReplayMetadata> Metadata => metaCache.Cache;
        public static IDictionary<string, SerializableReplayTag> Tags => tagsCache.Cache;

        public static void AddMetadataByPath(string path, IReplayMetadata metadata) {
            if (!ValidateMetadata(metadata)) return;
            Metadata[path] = ToSerializableReplayMetadata(metadata);
        }
        
        public static void AddTag(IReplayTag tag) {
            Tags[tag.Name] = ToSerializableReplayTag(tag);
        }

        private static bool ValidateMetadata(IReplayMetadata metadata) {
            return metadata.Tags.Count > 0;
        }
        
        private static SerializableReplayMetadata ToSerializableReplayMetadata(IReplayMetadata metadata) {
            return new() {
                Tags = metadata.Tags
                    .Select(x => x.Name)
                    .ToHashSet()
            };
        }

        private static SerializableReplayTag ToSerializableReplayTag(IReplayTag tag) {
            return new(tag.Name, tag.Color);
        }

        #endregion
    }
}