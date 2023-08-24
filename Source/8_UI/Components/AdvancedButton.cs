using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class AdvancedButton : LayoutComponentBase<AdvancedButton> {
        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action? ClickEvent;

        [ExternalProperty, UsedImplicitly]
        public event Action<bool>? ToggleEvent;

        #endregion

        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public bool Sticky {
            get => _sticky;
            set {
                _sticky = value;
                _isActive = false;
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
        public bool ColorizeOnHover {
            get => _colorizeOnHover;
            set {
                _colorizeOnHover = value;
                if (!IsInitialized) return;
                UpdateColor(0);
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
        public Color ActiveColor { get; set; } = DefaultHoveredColor;

        [ExternalProperty, UsedImplicitly]
        public Color HoveredColor { get; set; } = DefaultHoveredColor;

        [ExternalProperty, UsedImplicitly]
        public Color Color {
            get => _color;
            set {
                _color = value;
                UpdateColor(0);
            }
        }

        private bool _sticky;
        private bool _isActive;
        private bool _colorizeOnHover = true;
        private bool _growOnHover = true;
        private Color _color = DefaultColor;
        private float _lerpMul = 10f;

        #endregion

        #region UI Components

        [ExternalComponent, UsedImplicitly]
        [ExternalProperty(prefix: null,
            nameof(AdvancedImage.Icon),
            nameof(AdvancedImage.PreserveAspect))]
        public AdvancedImage Image { get; set; } = null!;

        private SmoothHoverController? _hoverController;

        #endregion

        #region Button

        public void Click(bool notifyListeners = false) {
            Sticky = false;
            HandleButtonClick(notifyListeners);
        }

        public void Toggle(bool state, bool notifyListeners = false) {
            Sticky = true;
            if (_isActive == state) return;
            _isActive = !state;
            HandleButtonClick(notifyListeners);
        }

        private void HandleButtonClick(bool notifyListeners) {
            if (Sticky) {
                _isActive = !_isActive;
                UpdateColor(_isActive ? 1 : 0);
                if (notifyListeners) ToggleEvent?.Invoke(_isActive);
            } else if (notifyListeners) ClickEvent?.Invoke();
        }

        #endregion

        #region Setup

        protected override void OnPropertySet() {
            if (BaseScale == Vector3.zero) BaseScale = Scale;
        }

        protected override void OnInitialize() {
            Image = AdvancedImage.Instantiate(ContentTransform!);
            Image.InheritSize = true;
            Image.Material = BundleLoader.UIAdditiveGlowMaterial;
            UpdateColor(0);
            _hoverController = Content!.AddComponent<SmoothHoverController>();
            _hoverController.HoverStateChangedEvent += OnHoverStateChanged;
            _hoverController.lerpCoefficient = _lerpMul;
            Content.AddComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        #endregion

        #region Colors

        public static readonly Color DefaultHoveredColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        public static readonly Color DefaultColor = new(0.8f, 0.8f, 0.8f, 0.2f);

        private void UpdateColor(float hoverProgress) {
            Image.Color = _isActive ? ActiveColor : Color.Lerp(Color, HoveredColor, hoverProgress);
        }

        #endregion

        #region Callbacks

        private void OnHoverStateChanged(bool isHovered, float progress) {
            var scale = BaseScale + HoverScaleSum * progress;
            if (_growOnHover) ContentTransform!.localScale = scale;
            if (_colorizeOnHover) UpdateColor(progress);
        }

        private void OnButtonClick() {
            HandleButtonClick(true);
        }

        #endregion
    }
}