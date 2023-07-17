using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components{
    internal class ReplayButton : ReeUIComponentV2 {
        public bool Interactable {
            get => _button.interactable;
            set => _button.interactable = value;
        }
        
        public event Action? ReplayButtonClickedEvent;

        [UIComponent("replay-button")]
        private readonly Transform _buttonTransform = null!;
        
        [UIComponent("replay-button")]
        private readonly Button _button = null!;

        private ImageView _backgroundImage = null!;
        private ImageView _foregroundImage = null!;

        protected override void OnInitialize() {
            SetupReplayButton();
        }

        private void SetupReplayButton() {
            var images = _buttonTransform.GetComponentsInChildren<ImageView>();

            _backgroundImage = images.First(x => x.gameObject.name == "BG");
            _foregroundImage = images.First(x => x.gameObject.name == "Icon");

            _foregroundImage.SetField("_skew", _backgroundImage.skew);
            _foregroundImage.__Refresh();
        }

        [UIAction("replay-button-clicked"), UsedImplicitly]
        private void HandleReplayButtonClicked() {
            ReplayButtonClickedEvent?.Invoke();
        }
    }
}
