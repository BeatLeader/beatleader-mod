using BeatSaberMarkupLanguage.Attributes;
using System;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ReplayFinishPanel : ReeUIComponentV2 {
        public event Action? ExitButtonClickedEvent;

        [UIObject("container-image")]
        private readonly GameObject _containerObject =null!;

        public void SetEnabled(bool enabled) {
            _containerObject.SetActive(enabled);
        }

        [UIAction("exit-button-clicked")]
        private void HandleExitButtonClicked() {
            ExitButtonClickedEvent?.Invoke();   
        }
    }
}
