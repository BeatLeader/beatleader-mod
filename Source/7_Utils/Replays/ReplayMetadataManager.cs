using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using BeatLeader.Models;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Utils {
    /// <summary>
    /// A class for managing replay metadatas.
    /// </summary>
    [PublicAPI]
    public static class ReplayMetadataManager {
        #region Tags

        public static event Action<ReplayTag>? TagCreatedEvent;
        public static event Action<ReplayTag>? TagDeletedEvent;
        public static event Action<ReplayTag>? TagUpdatedEvent;

        public static IReadOnlyDictionary<string, ReplayTag> Tags => tagsCache.Cache;

        private static IDictionary<string, ReplayTag> MutableTags => tagsCache.Cache;

        /// <summary>
        /// Validates the specified tag name.
        /// </summary>
        /// <param name="name">A name to validate.</param>
        /// <returns>A struct containing validation results.</returns>
        public static ReplayTagValidationResult ValidateTagName(string name) {
            return new(
                name.Length is >= 2 and <= 10,
                !MutableTags.ContainsKey(name)
            );
        }

        /// <summary>
        /// Updates name of an existing tag.
        /// </summary>
        /// <param name="name">The old name.</param>
        /// <param name="newName">A new name.</param>
        /// <exception cref="InvalidOperationException">If the tag does not exist.</exception>
        /// <returns>A struct containing validation results.</returns>
        public static ReplayTagValidationResult UpdateTagName(string name, string newName) {
            var val = ValidateTagName(newName);

            if (val.Ok) {
                if (!MutableTags.TryGetValue(name, out var tag)) {
                    throw new InvalidOperationException("The tag does not exist");
                }

                tag.Name = newName;

                MutableTags.Remove(name);
                MutableTags.Add(newName, tag);
            }

            return val;
        }

        /// <summary>
        /// Updates color of an existing tag.
        /// </summary>
        /// <param name="name">The name of the tag to update.</param>
        /// <exception cref="InvalidOperationException">If the tag does not exist.</exception>
        public static void UpdateTagColor(string name, Color newColor) {
            if (!MutableTags.TryGetValue(name, out var tag)) {
                throw new InvalidOperationException("The tag does not exist");
            }

            tag.Color = newColor;
            SynchronizationContext.Current.Send(_ => TagUpdatedEvent?.Invoke(tag), null);
        }

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <returns>A struct containing validation results.</returns>
        public static ReplayTagValidationResult CreateTag(string name, Color? color = null) {
            var val = ValidateTagName(name);

            if (val.Ok) {
                var tag = new ReplayTag(name, color ?? Color.white);

                MutableTags[name] = tag;
                SynchronizationContext.Current.Send(_ => TagCreatedEvent?.Invoke(tag), null);
            }

            return val;
        }

        /// <summary>
        /// Deletes an existing tag.
        /// </summary>
        /// <param name="name">The name of the tag to remove.</param>
        /// <exception cref="InvalidOperationException">If the tag does not exist.</exception>
        /// <returns>True if the tag was removed, otherwise False.</returns>
        public static void DeleteTag(string name) {
            if (!MutableTags.TryGetValue(name, out var tag)) {
                throw new InvalidOperationException("The tag does not exist");
            }

            MutableTags.Remove(name);
            SynchronizationContext.Current.Send(_ => TagDeletedEvent?.Invoke(tag), null);
        }

        #endregion

        #region Metadata

        internal static Dictionary<string, ReplayMetadata> MutableMetadatas => metaCache.Cache;

        internal static ReplayMetadata GetMetadata(string path) {
            var name = Path.GetFileName(path);

            if (!MutableMetadatas.TryGetValue(name, out var metadata)) {
                metadata = new ReplayMetadata();
                MutableMetadatas[name] = metadata;
            }

            return metadata;
        }

        internal static void DeleteMetadata(string path) {
            MutableMetadatas.Remove(Path.GetFileName(path));
        }

        internal static void ClearMetadata() {
            MutableMetadatas.Clear();
        }

        #endregion

        #region Serialization

        private static readonly AppCache<Dictionary<string, ReplayTag>> tagsCache = new("ReplayTagsCache");
        private static readonly AppCache<Dictionary<string, ReplayMetadata>> metaCache = new("ReplayMetadataCache");

        internal static void LoadCache() {
            tagsCache.Load();
            // Important to load after tags as it takes tags instances from ReplayMetadataManager
            metaCache.Load();
        }

        internal static void SaveCache() {
            tagsCache.Save();
            metaCache.Save();
        }

        #endregion
    }
}