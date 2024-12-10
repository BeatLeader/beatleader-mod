using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class ChristmasTreeMover : ReeUIComponentV2 {
        [UIComponent("modal"), UsedImplicitly]
        private ModalView _modal = null!;

        public void Present() {
            _modal.Show(true, true);
        }

        [UIAction("finish-click"), UsedImplicitly]
        private void HandleFinishButtonClicked() {
            _modal.Hide(true);
        }

        protected override void OnInitialize() {
            
        }
    }
}