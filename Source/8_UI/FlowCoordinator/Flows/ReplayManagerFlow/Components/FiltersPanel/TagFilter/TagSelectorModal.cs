using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;

namespace BeatLeader.UI.Hub {
    internal class TagSelectorModal : ModalBase {
        public TagSelector Component => _tagSelector;

        private TagSelector _tagSelector = null!;

        protected override IReactiveComponent ConstructContent() {
            return new TagSelector().Bind(ref _tagSelector).InShadow();
        }
    }
}