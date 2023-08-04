using System;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BeatLeader.Components {
    internal class HoverText : ReeUIComponentV2 {
        #region StateListener

        private class PointerStateListener : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
            public event Action<bool>? PointerStateChangedEvent;

            public void OnPointerEnter(PointerEventData eventData) {
                PointerStateChangedEvent?.Invoke(true);
            }

            public void OnPointerExit(PointerEventData eventData) {
                PointerStateChangedEvent?.Invoke(false);
            }
        }

        #endregion

        #region UI Components

        [UIComponent("text"), UsedImplicitly]
        private TMP_Text? _text;

        public TMP_Text TextObject {
            get {
                ValidateAndThrow();
                return _text!;
            }
        }

        public bool HoverEnabled {
            get => TextObject.raycastTarget;
            set => TextObject.raycastTarget = value;
        }

        #endregion

        #region UpdateVisuals

        private void UpdateVisuals() {
            _text!.text = _isHovered ? _text2 : _text1;
        }

        #endregion
        
        #region Init

        public string? Text1 {
            get => _text1;
            set {
                _text1 = value;
                if (!IsParsed) return;
                UpdateVisuals();
            }
        }

        public string? Text2 {
            get => _text2;
            set {
                _text2 = value;
                if (!IsParsed) return;
                UpdateVisuals();
            }
        }
        
        private string? _text1;
        private string? _text2;
        private bool _isHovered;
        
        protected override void OnInitialize() {
            _text!.gameObject.AddComponent<PointerStateListener>().PointerStateChangedEvent += HandlePointerStateChanged;
            HoverEnabled = true;
        }

        #endregion

        #region Callbacks

        private void HandlePointerStateChanged(bool hovered) {
            _isHovered = hovered;
            UpdateVisuals();
        }

        #endregion
    }
}