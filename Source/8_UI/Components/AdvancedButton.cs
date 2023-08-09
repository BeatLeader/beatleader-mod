using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class AdvancedButton : ComponentLayoutBase<AdvancedButton> {
        #region Events

        [ExternalProperty, UsedImplicitly]
        public event Action? ClickEvent;

        #endregion

        #region UI Properties

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
        public bool HighlightOnHover {
            get => _highlightOnHover;
            set {
                _highlightOnHover = value;
                UpdateColor(0);
            }
        }

        [ExternalProperty, UsedImplicitly]
        public Color HoverColor { get; set; } = hoveredColor;

        [ExternalProperty, UsedImplicitly]
        public Color Color {
            get => _color;
            set {
                _color = value;
                UpdateColor(0);
            }
        }

        private bool _highlightOnHover = true;
        private bool _growOnHover = true;
        private Color _color = standardColor;

        #endregion

        #region UI Components

        [ExternalComponent, UsedImplicitly]
        [ExternalProperty(prefix: null, 
            nameof(AdvancedImage.Icon), 
            nameof(AdvancedImage.PreserveAspect))]
        private AdvancedImage _image = null!;

        #endregion

        #region Setup

        protected override void OnInitialize() {
            _image = AdvancedImage.Instantiate(ContentTransform!);
            _image.InheritSize = true;
            _image.Material = BundleLoader.UIAdditiveGlowMaterial;
            var hoverController = Content!.AddComponent<SmoothHoverController>();
            hoverController.HoverStateChangedEvent += OnHoverStateChanged;
            Content.AddComponent<Button>().onClick.AddListener(OnButtonClick);
        }

        #endregion

        #region Colors

        private static readonly Color hoveredColor = new(0.0f, 0.4f, 1.0f, 1.0f);
        private static readonly Color standardColor = new(0.8f, 0.8f, 0.8f, 0.2f);

        private void UpdateColor(float hoverProgress) {
            _image.Color = Color.Lerp(standardColor, hoveredColor, hoverProgress);
        }

        #endregion

        #region Callbacks

        private void OnHoverStateChanged(bool isHovered, float progress) {
            var scale = 1.0f + 0.2f * progress;
            if (_growOnHover) ContentTransform!.localScale = new Vector3(scale, scale, scale);
            if (_highlightOnHover) UpdateColor(progress);
        }

        private void OnButtonClick() {
            ClickEvent?.Invoke();
        }

        #endregion
    }
}