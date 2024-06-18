using System;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class CloseButton : ReeUIComponentV2 {
        //static CloseButton() => ColorUtility.TryParseHtmlString("#00000080", out inactiveColor);

        private static readonly Color inactiveColor;
        private static readonly Color highlightedColor = Color.red.ColorWithAlpha(0.8f);

        public event Action? ButtonPressedEvent;

        [UIComponent("button")]
        private readonly NoTransitionsButton _button = null!;

        private ImageView _background = null!;

        protected override void OnInitialize() {
            _button.GetComponent<ButtonStaticAnimations>().TryDestroy();
            _button.selectionStateDidChangeEvent += HandleButtonStateChanged;
            var buttonTransform = _button.transform;
            buttonTransform.Find("Underline")?.gameObject.SetActive(false);
            _background = buttonTransform.Find("BG").GetComponent<ImageView>();
        }

        [UIAction("click"), UsedImplicitly]
        private void HandleButtonClicked() {
            ButtonPressedEvent?.Invoke();
        }

        private void HandleButtonStateChanged(NoTransitionsButton.SelectionState state) {
            _background.color = state is NoTransitionsButton.SelectionState.Highlighted ? highlightedColor : inactiveColor;
        }
    }
}