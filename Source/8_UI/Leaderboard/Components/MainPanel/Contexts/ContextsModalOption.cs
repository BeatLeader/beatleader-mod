using System;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class ContextsModalOption : ReeUIComponentV2 {
        #region Components

        [UIComponent("button"), UsedImplicitly]
        private Button _button;

        [UIComponent("button"), UsedImplicitly]
        private TextMeshProUGUI _buttonText;

        protected override void OnInitialize() {
            _button.onClick.AddListener(() => OnClick?.Invoke());
            PluginConfig.ScoresContextChangedEvent += OnContextChanged;
            UpdateVisuals();
        }

        protected override void OnDispose() {
            PluginConfig.ScoresContextChangedEvent -= OnContextChanged;
        }

        private void OnContextChanged(int value) {
            UpdateVisuals();
        }

        #endregion

        #region Context

        public event Action OnClick;

        private ScoresContext? _context = null;

        public void SetContext(ScoresContext value) {
            _context = value;
            UpdateVisuals();
        }

        private void UpdateVisuals() {
            if (!IsParsed || _context == null) return;

            _buttonText.text = _context.Name;
            HoverHint = _context.Description;

            if (_context.Id == PluginConfig.ScoresContext) {
                _buttonText.faceColor = Color.cyan;
                _buttonText.fontSize = 4.0f;
            } else {
                _buttonText.faceColor = Color.white;
                _buttonText.fontSize = 3.5f;
            }
        }

        #endregion

        #region HoverHint

        private string _hoverHint = "";

        [UIValue("hover-hint"), UsedImplicitly]
        public string HoverHint {
            get => _hoverHint;
            set {
                if (_hoverHint.Equals(value)) return;
                _hoverHint = value;
                NotifyPropertyChanged();
            }
        }

        #endregion
    }
}