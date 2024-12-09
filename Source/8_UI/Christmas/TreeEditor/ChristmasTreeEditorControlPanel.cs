using System;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;

namespace BeatLeader.Components {
    internal class ChristmasTreeEditorControlPanel : ReeUIComponentV2 {
        public event Action? CancelButtonClickedEvent;
        public event Action? OkButtonClickedEvent;
        
        private StaticScreen _screen = null!;

        public void Present() {
            _screen.Present();
        }

        public void Dismiss() {
            _screen.Dismiss();
        }
        
        protected override void OnInitialize() {
            _screen = Content.gameObject.AddComponent<StaticScreen>();
        }

        [UIAction("cancel-click"), UsedImplicitly]
        private void HandleCancelClicked() {
            CancelButtonClickedEvent?.Invoke();
        }
        
        [UIAction("save-click"), UsedImplicitly]
        private void HandleSaveClicked() {
            OkButtonClickedEvent?.Invoke();
        }
    }
}