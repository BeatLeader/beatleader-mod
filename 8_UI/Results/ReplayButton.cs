using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using System;
using System.Linq;
using UnityEngine;

namespace BeatLeader.Components{
    internal class ReplayButton : ReeUIComponentV2 {
        public event Action ReplayButtonClickedEvent;

        [UIComponent("replay-button")]
        private readonly Transform _buttonTransform;

        private ImageView _backgroundImage;
        private ImageView _foregroundImage;

        protected override void OnInitialize() {
            SetupReplayButton();
        }

        private void SetupReplayButton() {
            var images = _buttonTransform.GetComponentsInChildren<ImageView>();
            Debug.Log("Images " + images.Length);
            _backgroundImage = images.FirstOrDefault(x => x.gameObject.name == "BG");
            _foregroundImage = images.FirstOrDefault(x => x.gameObject.name == "Icon");

            _foregroundImage.SetField("_skew", _backgroundImage.skew);
            _foregroundImage.__Refresh();
        }

        [UIAction("replay-button-clicked"), UsedImplicitly]
        private void HandleReplayButtonClicked() {
            ReplayButtonClickedEvent?.Invoke();
        }
    }
}
