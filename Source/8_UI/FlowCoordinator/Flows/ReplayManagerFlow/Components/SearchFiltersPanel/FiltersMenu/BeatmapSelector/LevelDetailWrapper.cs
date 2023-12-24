using System;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class LevelDetailWrapper : ReeUIComponentV2 {
        public Vector2 Size {
            get => _rectTransform.sizeDelta;
            set => _rectTransform.sizeDelta = value;
        }
        
        public event Action? SelectButtonPressedEvent;

        [UIComponent("container")]
        private readonly Transform _container = null!;

        private Transform? _additionalInfo;
        private RectTransform _rectTransform = null!;
        
        public void Setup(Transform? additionalInfo) {
            if (additionalInfo == null) {
                _additionalInfo?.SetParent(null, false);
                _additionalInfo = null;
                return;
            }
            additionalInfo.SetParent(_container, false);
            additionalInfo.SetSiblingIndex(0);
            _additionalInfo = additionalInfo;
        }

        protected override void OnInstantiate() {
            _rectTransform = gameObject.AddComponent<RectTransform>();
        }

        [UIAction("on-click"), UsedImplicitly]
        private void HandleButtonClicked() {
            SelectButtonPressedEvent?.Invoke();
        }
    }
}