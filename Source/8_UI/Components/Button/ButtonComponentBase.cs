using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal abstract class ButtonComponentBase<T> : LayoutComponentBase<T> where T : ReeUIComponentV3<T> {
        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action<bool>? ClickEvent;

        #endregion

        #region UI Properties

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
                ContentTransform!.localScale = Vector3.one;
            }
        }

        [ExternalProperty, UsedImplicitly]
        public float HoverLerpMul {
            get => _hoverController?.lerpCoefficient ?? -1f;
            set {
                _lerpMul = value;
                if (!IsInitialized) return;
                _hoverController!.lerpCoefficient = _lerpMul;
            }
        }

        [ExternalProperty, UsedImplicitly]
        public Vector3 HoverScaleSum { get; set; } = new(0.2f, 0.2f, 0.2f);

        [ExternalProperty, UsedImplicitly]
        public Vector3 BaseScale { get; set; } = Vector3.zero;

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
            if (notifyListeners) ClickEvent?.Invoke(_sticky ? IsActive : default);
        }

        #endregion

        #region Abstraction

        protected virtual void OnHoverProgressChange(float progress) { }

        protected virtual void OnButtonStateChange(bool state) { }

        #endregion

        #region Setup

        private SmoothHoverController? _hoverController;
        private bool _isActive;

        protected abstract void OnContentConstruct(Transform parent);

        protected override void OnPropertySet() {
            if (BaseScale == Vector3.zero) BaseScale = Scale;
        }

        protected sealed override void OnInitialize() {
            OnContentConstruct(ContentTransform!);
            OnHoverProgressChange(0);
            _hoverController = Content!.AddComponent<SmoothHoverController>();
            _hoverController.HoverStateChangedEvent += OnHoverStateChanged;
            _hoverController.lerpCoefficient = _lerpMul;
            Content.AddComponent<Button>().onClick.AddListener(OnButtonClick);
            OnInitializeInternal();
        }

        protected virtual void OnInitializeInternal() { }

        #endregion

        #region Callbacks

        private void OnHoverStateChanged(bool isHovered, float progress) {
            if (_growOnHover) ContentTransform!.localScale = BaseScale + HoverScaleSum * progress;
            OnHoverProgressChange(progress);
        }

        private void OnButtonClick() {
            ProcessButtonClick(true);
        }

        #endregion
    }
}