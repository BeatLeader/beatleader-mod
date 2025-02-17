using System;
using System.Collections.Generic;

namespace BeatLeader.Models {
    internal class GenericReplayMetadata : IReplayMetadata {
        public GenericReplayMetadata() {
            var set = new ObservableSet<IReplayTag>();
            set.ItemAddedEvent += HandleTagAdded;
            set.ItemRemovedEvent += HandleTagRemoved;
            Tags = set;
        }

        public ICollection<IReplayTag> Tags { get; }

        public event Action<IReplayTag>? TagAddedEvent;
        public event Action<IReplayTag>? TagRemovedEvent;

        private void HandleTagAdded(IReplayTag tag) {
            TagAddedEvent?.Invoke(tag);
        }

        private void HandleTagRemoved(IReplayTag tag) {
            TagRemovedEvent?.Invoke(tag);
        }
    }
}