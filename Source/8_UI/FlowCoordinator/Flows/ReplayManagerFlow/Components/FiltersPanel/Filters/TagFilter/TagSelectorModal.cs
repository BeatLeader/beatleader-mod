using Reactive.Components;

namespace BeatLeader.UI.Hub {
    internal class TagSelectorModal : SharedModal<ModalWrapper<TagSelector>> {
        public TagSelector Component => Modal.Component;
    }
}