using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal abstract class ButtonComponentBase<T> : LayoutComponentBase<T> where T : ReeUIComponentV3<T> {
        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action? ClickEvent;

        [ExternalProperty, UsedImplicitly]
        public event Action<bool>? StateChangedEvent;

        #endregion

        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public bool Interactable {
            get => _button.interactable;
            set {
                _button.interactable = value;
                _hoverController.enabled = value;
                OnInteractableChange(value);
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool Sticky {
            get => _sticky;
            set {
                _sticky = value;
                IsActive = false;
            }
        }

        [ExternalProperty, UsedImplicitly]
        public bool GrowOnHover {
            get => _growOnHover;
            set {
                _growOnHover = value;
                if (!IsInitialized) return;
                ContentTransform.localScale = Vector3.one;
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float HoverLerpMul {
            get => _hoverController.lerpCoefficient;
            set {
                _lerpMul = value;
                if (!IsInitialized) return;
                _hoverController.lerpCoefficient = _lerpMul;
            }
        }

        [ExternalProperty, UsedImplicitly]
        public Vector3 HoverScaleSum { get; set; } = new(0.2f, 0.2f, 0.2f);

        [ExternalProperty, UsedImplicitly]
        public Vector3 BaseScale { get; set; } = Vector3.one;

        [ExternalProperty, UsedImplicitly]
        public bool IsActive {
            get => _isActive;
            private set {
                if (value == _isActive) return;
                OnButtonStateChange(_isActive = value);
            }
        }

        private bool _sticky;
        private bool _growOnHover = true;
        private float _lerpMul = 10f;

        #endregion

        #region Button

        /// <summary>
        /// Emulates UI button click
        /// </summary>
        /// <param name="state">Determines the toggle state. Valid only if <c>Sticky</c> is turned on</param>
        /// <param name="notifyListeners">Determines should event be invoked or not</param>
        public void Click(bool state = false, bool notifyListeners = false) {
            if (Sticky) _isActive = !state;
            ProcessButtonClick(notifyListeners);
        }

        private void ProcessButtonClick(bool notifyListeners) {
            if (Sticky) IsActive = !IsActive;
            if (!notifyListeners) return;
            ClickEvent?.Invoke();
            StateChangedEvent?.Invoke(_sticky ? IsActive : default);
        }

        #endregion

        #region Abstraction

        protected virtual void OnHoverProgressChange(float progress) { }

        protected virtual void OnButtonStateChange(bool state) { }

        protected virtual void OnInteractableChange(bool interactable) { }
        
        #endregion

        #region Setup

        private SmoothHoverController _hoverController = null!;
        private Button _button = null!;
        private bool _isActive;

        protected abstract void OnContentConstruct(Transform parent);

        protected sealed override void OnConstruct(Transform parent) {
            var parentGo = parent.gameObject;
            OnContentConstruct(parent);
            OnHoverProgressChange(0);
            _hoverController = parentGo.AddComponent<SmoothHoverController>();
            _hoverController.AddStateListener(OnHoverStateChanged, false);
            _hoverController.lerpCoefficient = _lerpMul;
            _button = parentGo.AddComponent<Button>();
            _button.onClick.AddListener(OnButtonClick);
        }

        #endregion

        #region Callbacks

        private void OnHoverStateChanged(bool isHovered, float progress) {
            if (_growOnHover) ContentTransform.localScale = BaseScale + HoverScaleSum * progress;
            OnHoverProgressChange(progress);
        }

        private void OnButtonClick() {
            ProcessButtonClick(true);
        }

        #endregion
    }
}