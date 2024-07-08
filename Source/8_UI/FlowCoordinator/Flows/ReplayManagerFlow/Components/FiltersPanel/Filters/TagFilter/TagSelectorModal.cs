using BeatLeader.UI.Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal class TagSelectorModal : SharedModal<AnimatedModalWrapper<TagSelector>> {
        public TagSelector Component => Modal.Component;
    }
}