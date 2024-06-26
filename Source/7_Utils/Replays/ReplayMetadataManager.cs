using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatLeader.Models;
using IPA.Utilities;

namespace BeatLeader.Utils {
    internal class ReplayMetadataManager : Singleton<ReplayMetadataManager>, IReplayMetadataManager, IReplayTagManager {
        #region MetadataManager

        public IReplayTagManager TagManager => this;

        #endregion

        #region TagManager

        public IReadOnlyCollection<IEditableReplayTag> Tags => cachedTags.Values;

        public event Action<IEditableReplayTag>? TagCreatedEvent;
        public event Action<IReplayTag>? TagDeletedEvent;

        public ReplayTagValidationResult ValidateTag(string name) {
            return new(
                name.Length is >= 2 and <= 10,
                !cachedTags.ContainsKey(name)
            );
        }

        public IEditableReplayTag CreateTag(string name) {
            var tag = new GenericReplayTag(name, ValidateTag, DeleteTag);
            cachedTags[name] = tag;
            TagCreatedEvent?.Invoke(tag);
            return tag;
        }

        public void DeleteTag(IReplayTag tag) {
            cachedTags.Remove(tag.Name);
            TagDeletedEvent?.Invoke(tag);
        }

        #endregion

        #region Serialization

        private static readonly Dictionary<string, IEditableReplayTag> cachedTags = new();
        private static readonly Dictionary<string, IReplayMetadata> cachedMetadatas = new();

        public static void LoadSerializedCache() {
            LoadTags();
            LoadMetadatas();
        }

        public static void SaveSerializedCache() {
            SaveTags();
            SaveMetadatas();
        }

        private static void LoadTags() {
            foreach (var (_, serializedTag) in ReplayHeadersCache.Tags) {
                var tag = ToGenericReplayTag(serializedTag);
                cachedTags.Add(tag.Name, tag);
            }
        }

        private static void SaveTags() {
            foreach (var (_, tag) in cachedTags) {
                ReplayHeadersCache.AddTag(tag);
            }
        }

        private static void LoadMetadatas() {
            foreach (var (path, serializedMetadata) in ReplayHeadersCache.Metadata) {
                var meta = new GenericReplayMetadata();
                var tags = serializedMetadata.Tags
                    .Where(x => cachedTags.ContainsKey(x))
                    .Select(x => cachedTags[x]);
                meta.Tags.AddRange(tags);
                cachedMetadatas[path] = meta;
            }
        }

        private static void SaveMetadatas() {
            foreach (var (path, metadata) in cachedMetadatas) {
                ReplayHeadersCache.AddMetadataByPath(path, metadata);
            }
        }

        private static GenericReplayTag ToGenericReplayTag(SerializableReplayTag tag) {
            var stag = new GenericReplayTag(tag.Name, Instance.ValidateTag, Instance.DeleteTag);
            stag.SetColor(tag.Color);
            return stag;
        }

        #endregion

        #region Internal

        public static IReplayMetadata GetMetadata(string path) {
            var name = Path.GetFileName(path);
            if (!cachedMetadatas.TryGetValue(name, out var metadata)) {
                metadata = new GenericReplayMetadata();
                cachedMetadatas[name] = metadata;
            }
            return metadata;
        }

        public static void DeleteMetadata(string path) {
            cachedMetadatas.Remove(Path.GetFileName(path));
        }

        public static void ClearMetadata() {
            cachedMetadatas.Clear();
        }

        #endregion
    }
}