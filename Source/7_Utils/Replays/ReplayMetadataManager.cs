using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using BeatLeader.Models;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
            lock (tagsLocker) {
                return new(
                    name.Length is >= 2 and <= 10,
                    !MutableTags.ContainsKey(name)
                );
            }
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
                lock (tagsLocker) {
                    if (!MutableTags.TryGetValue(name, out var tag)) {
                        throw new InvalidOperationException("The tag does not exist");
                    }

                    tag.Name = newName;

                    MutableTags.Remove(name);
                    MutableTags.Add(newName, tag);
                }
            }

            return val;
        }

        /// <summary>
        /// Updates color of an existing tag.
        /// </summary>
        /// <param name="name">The name of the tag to update.</param>
        /// <exception cref="InvalidOperationException">If the tag does not exist.</exception>
        public static void UpdateTagColor(string name, Color newColor) {
            lock (tagsLocker) {
                if (!MutableTags.TryGetValue(name, out var tag)) {
                    throw new InvalidOperationException("The tag does not exist");
                }

                tag.Color = newColor;
                SynchronizationContext.Current.Post(static x => TagUpdatedEvent?.Invoke((ReplayTag)x), tag);
            }
        }

        /// <summary>
        /// Creates a new tag.
        /// </summary>
        /// <returns>A struct containing validation results.</returns>
        public static ReplayTagValidationResult CreateTag(string name, Color? color = null) {
            var val = ValidateTagName(name);

            if (val.Ok) {
                var tag = new ReplayTag(name, color ?? Color.white);

                lock (tagsLocker) {
                    MutableTags[name] = tag;
                }

                NotifyTagCreated(tag);
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
            lock (tagsLocker) {
                if (!MutableTags.TryGetValue(name, out var tag)) {
                    throw new InvalidOperationException("The tag does not exist");
                }

                MutableTags.Remove(name);
                NotifyTagDeleted(tag);
            }
        }

        #endregion

        #region Events

        private static void NotifyTagCreated(ReplayTag tag) {
            SynchronizationContext.Current.Post(static x => TagCreatedEvent?.Invoke((ReplayTag)x), tag);
        }

        private static void NotifyTagDeleted(ReplayTag tag) {
            SynchronizationContext.Current.Post(static x => {
                var tag = (ReplayTag)x;

                TagDeletedEvent?.Invoke(tag);

                foreach (var metadata in MutableMetadatas.Values) {
                    metadata.NotifyTagDeleted(tag);
                }
            }, tag);
        }

        #endregion

        #region Metadata

        internal static Dictionary<string, ReplayMetadata> MutableMetadatas => metaCache.Cache;

        internal static ReplayMetadata GetMetadata(string path) {
            var name = Path.GetFileName(path);

            lock (metaLocker) {
                if (!MutableMetadatas.TryGetValue(name, out var metadata)) {
                    metadata = new ReplayMetadata();
                    MutableMetadatas[name] = metadata;
                }

                return metadata;
            }
        }

        internal static void DeleteMetadata(string path) {
            lock (metaLocker) {
                MutableMetadatas.Remove(Path.GetFileName(path));
            }
        }

        internal static void ClearMetadata() {
            lock (metaLocker) {
                MutableMetadatas.Clear();
            }
        }

        #endregion

        #region Serialization

        private class MetadataConverter : JsonConverter {
            public override bool CanConvert(Type objectType) {
                return objectType == typeof(Dictionary<string, ReplayMetadata>);
            }

            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) {
                var dict = (Dictionary<string, ReplayMetadata>)value!;

                writer.WriteStartObject();
                foreach (var pair in dict) {
                    // Only write the key if the Tags collection has items
                    if (pair.Value.Tags is { Count: > 0 }) {
                        writer.WritePropertyName(pair.Key);
                        serializer.Serialize(writer, pair.Value);
                    }
                }
                writer.WriteEndObject();
            }

            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
                if (reader.TokenType == JsonToken.Null) {
                    return null;
                }

                var result = new Dictionary<string, ReplayMetadata>();

                while (reader.Read()) {
                    if (reader.TokenType == JsonToken.EndObject) {
                        break;
                    }

                    if (reader.TokenType == JsonToken.PropertyName) {
                        var key = reader.Value!.ToString()!;

                        reader.Read();

                        var jo = JObject.Load(reader);
                        var tagsToken = jo["Tags"];

                        if (tagsToken is not { HasValues: true }) {
                            continue;
                        }

                        var metadata = jo.ToObject<ReplayMetadata>(serializer);

                        if (metadata != null) {
                            result.Add(key, metadata);
                        }
                    }
                }

                return result;
            }
        }

        private static readonly AppCache<Dictionary<string, ReplayTag>> tagsCache = new("ReplayTagsCache");
        private static readonly AppCache<Dictionary<string, ReplayMetadata>> metaCache = new("ReplayMetadataCache", new() {
            Converters = [new MetadataConverter()]
        });

        private static readonly object tagsLocker = new();
        private static readonly object metaLocker = new();

        internal static void LoadCache() {
            lock (tagsLocker) {
                tagsCache.Load();
            }
            // Important to load after tags as it takes tags instances from ReplayMetadataManager
            lock (metaLocker) {
                metaCache.Load();
            }
        }

        internal static void SaveCache() {
            lock (tagsCache) {
                tagsCache.Save();
            }
            lock (metaLocker) {
                metaCache.Save();
            }
        }

        #endregion
    }
}