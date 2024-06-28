using System;
using BeatLeader.Components;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.UI.Reactive.Components {
    internal class ButtonBase : DrivingReactiveComponentBase, IAnimationProgressProvider, IGraphicComponent {
        #region Events

        public event Action? ClickEvent;

        public event Action<bool>? StateChangedEvent;

        #endregion

        #region UI Properties

        public bool RaycastTarget {
            get => _button.enabled;
            set {
                _button.enabled = value;
                _hoverController.enabled = value;
            }
        }

        public bool Interactable {
            get => _button.interactable;
            set {
                _button.interactable = value;
                _hoverController.enabled = value;
                OnInteractableChange(value);
                NotifyPropertyChanged();
            }
        }

        public bool Sticky {
            get => _sticky;
            set {
                _sticky = value;
                Active = false;
                NotifyPropertyChanged();
            }
        }

        public bool GrowOnHover {
            get => _growOnHover;
            set {
                _growOnHover = value;
                if (!IsInitialized) return;
                ContentTransform.localScale = Vector3.one;
                NotifyPropertyChanged();
            }
        }

        public float HoverLerpMul {
            get => _hoverController.lerpCoefficient;
            set {
                _lerpMul = value;
                if (!IsInitialized) return;
                _hoverController.lerpCoefficient = _lerpMul;
                NotifyPropertyChanged();
            }
        }

        public Vector3 HoverScaleSum {
            get => _hoverScaleSum;
            set {
                _hoverScaleSum = value;
                NotifyPropertyChanged();
            }
        }

        public Vector3 BaseScale {
            get => _baseScale;
            set {
                _baseScale = value;
                NotifyPropertyChanged();
            }
        }

        public bool Active {
            get => _buttonActive && Interactable;
            private set {
                if (value == _buttonActive) return;
                OnButtonStateChange(_buttonActive = value);
                NotifyPropertyChanged();
            }
        }

        public float AnimationProgress => _hoverController.Progress;

        private bool _sticky;
        private bool _growOnHover = true;
        private float _lerpMul = 10f;
        private Vector3 _hoverScaleSum = new(0.2f, 0.2f, 0.2f);
        private Vector3 _baseScale = Vector3.one;

        #endregion

        #region Button

        /// <summary>
        /// Emulates UI button click
        /// </summary>
        /// <param name="state">Determines the toggle state. Valid only if <c>Sticky</c> is turned on</param>
        /// <param name="notifyListeners">Determines should event be invoked or not</param>
        /// <param name="force">Determines should the state be changed or not even if it is the same</param>
        public void Click(bool state = false, bool notifyListeners = false, bool force = false) {
            if (!Interactable) return;
            if (Sticky) {
                if (!force && state == _buttonActive) return;
                _buttonActive = !state;
            }
            ProcessButtonClick(notifyListeners);
        }

        private void ProcessButtonClick(bool notifyListeners) {
            if (Sticky) Active = !Active;
            if (!notifyListeners) return;
            ClickEvent?.Invoke();
            StateChangedEvent?.Invoke(_sticky ? Active : default);
        }

        #endregion

        #region Abstraction

        protected virtual void OnHoverProgressChange(float progress) { }

        protected virtual void OnButtonStateChange(bool state) { }

        protected virtual void OnInteractableChange(bool interactable) { }

        #endregion

        #region Setup

        private SmoothHoverController _hoverController = null!;
        private UnityEngine.UI.Button _button = null!;
        private bool _buttonActive;

        protected override void Construct(RectTransform rect) {
            var go = rect.gameObject;
            _hoverController = go.AddComponent<SmoothHoverController>();
            _hoverController.AddStateListener(OnHoverStateChanged, false);
            _hoverController.lerpCoefficient = _lerpMul;
            _button = go.AddComponent<UnityEngine.UI.Button>();
            _button.navigation = new() { mode = Navigation.Mode.None };
            _button.onClick.AddListener(OnButtonClick);
            OnHoverProgressChange(0);
        }

        #endregion

        #region Callbacks

        private void OnHoverStateChanged(bool isHovered, float progress) {
            if (_growOnHover) ContentTransform.localScale = BaseScale + HoverScaleSum * progress;
            OnHoverProgressChange(progress);
            NotifyPropertyChanged(nameof(AnimationProgress));
        }

        private void OnButtonClick() {
            ProcessButtonClick(true);
            GameResources.ClickSignal.Raise();
        }

        #endregion
    }
}