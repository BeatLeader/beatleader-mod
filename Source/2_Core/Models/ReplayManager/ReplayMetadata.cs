using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BeatLeader.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayMetadata {
        [JsonConstructor]
        public ReplayMetadata() {
            _tags = new();
            var set = new ObservableCollectionAdapter<ReplayTag>(_tags);

            set.ItemAddedEvent += HandleTagAdded;
            set.ItemRemovedEvent += HandleTagRemoved;

            Tags = set;
        }

        public ICollection<ReplayTag> Tags { get; }

        [JsonProperty("Tags")]
        private HashSet<string>? _serializableTags;
        private readonly HashSet<ReplayTag> _tags;

        public event Action<ReplayTag>? TagAddedEvent;
        public event Action<ReplayTag>? TagRemovedEvent;

        private void HandleTagAdded(ReplayTag tag) {
            TagAddedEvent?.Invoke(tag);
        }

        private void HandleTagRemoved(ReplayTag tag) {
            TagRemovedEvent?.Invoke(tag);
        }

        [OnSerializing]
        private void OnSerializing() {
            _serializableTags ??= new();

            foreach (var tag in Tags) {
                _serializableTags.Add(tag.Name);
            }
        }

        [OnDeserialized]
        private void OnDeserialized() {
            if (_serializableTags == null) {
                return;
            }

            foreach (var tagName in _serializableTags) {
                if (ReplayMetadataManager.Tags.TryGetValue(tagName, out var tag)) {
                    _tags.Add(tag);
                } else {
                    Plugin.Log.Warn($"Found missing tag \"{tagName}\" when loading metadata");
                }
            }
        }
    }
}