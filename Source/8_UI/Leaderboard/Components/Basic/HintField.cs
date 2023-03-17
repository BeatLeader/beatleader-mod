using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class HintField : ReeUIComponentV2 {
        #region Components

        [UIComponent("root-component"), UsedImplicitly]
        private RectTransform _root;

        [UIComponent("root-component"), UsedImplicitly]
        private ImageView _backgroundImage;

        [UIComponent("text-component"), UsedImplicitly]
        private TextMeshProUGUI _textComponent;

        #endregion

        #region Handlers

        private readonly List<HintHandler> _handlers = new();

        public HintHandler RegisterHandler() {
            var handler = new HintHandler();
            _handlers.Add(handler);
            handler.WasTouchedEvent += UpdateTargetHint;
            return handler;
        }

        public class HintHandler {
            public string Hint { get; private set; }
            public bool IsActive { get; private set; }
            public int Priority { get; private set; }

            public event Action WasTouchedEvent;

            public void ShowHint([NotNull] string hint, int priority = 0) {
                if (IsActive) return;

                Hint = hint;
                Priority = priority;
                IsActive = true;
                WasTouchedEvent?.Invoke();
            }

            public void HideHint() {
                if (!IsActive) return;

                IsActive = false;
                WasTouchedEvent?.Invoke();
            }
        }

        #endregion

        #region Setup

        private string _defaultHint;

        protected override void OnInitialize() {
            UpdateTargetHint();
        }

        public void Setup(string defaultHint) {
            _defaultHint = defaultHint;
            UpdateTargetHint();
        }

        #endregion

        #region TargetHint

        private string _targetHint;

        private void UpdateTargetHint() {
            var hint = _defaultHint;
            var maxPriority = int.MinValue;

            foreach (var handler in _handlers) {
                if (!handler.IsActive || handler.Priority < maxPriority) continue;
                maxPriority = handler.Priority;
                hint = handler.Hint;
            }

            if (hint != _targetHint) {
                _targetHint = hint;
                _progress = 0.0f;
            }

            UpdateVisuals();
        }

        #endregion

        #region Animation

        private const float LerpCoefficient = 10.0f;
        private const float SnapBorder = 1.0f - 1e-5f;

        private float _progress = 1.0f;

        private void Update() {
            if (_progress >= 1) return;

            _progress = _progress > SnapBorder ? 1.0f : Mathf.Lerp(_progress, 1.0f, Time.deltaTime * LerpCoefficient);

            UpdateVisuals();
        }

        private void UpdateVisuals() {
            if (_progress > 0.5f) {
                _textComponent.text = _targetHint ?? "";
            }

            var t = Mathf.Abs((_progress - 0.5f) * 2.0f);
            _textComponent.alpha = t;
            _root.localScale = new Vector3(1.0f, t, 1.0f);
            _backgroundImage.color = _backgroundImage.color.ColorWithAlpha(t);
        }

        #endregion
    }
}