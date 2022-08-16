using System;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class QualificationCheckbox : ReeUIComponentV2 {
        #region Initialize

        protected override void OnInitialize() {
            InitializeMaterial();
            SetState(State.Neutral);

            _imageComponent.raycastTarget = true;
        }

        #endregion

        #region State

        private static readonly Color NeutralColor = new(0.6f, 0.6f, 0.6f, 0.3f);
        private static readonly Color FailedColor = new(1.0f, 0.0f, 0.0f, 0.8f);
        private static readonly Color CheckedColor = new(0.0f, 1.0f, 0.0f, 0.8f);

        public void SetState(State state) {
            _imageComponent.color = state switch {
                State.Neutral => NeutralColor,
                State.Failed => FailedColor,
                State.Checked => CheckedColor,
                _ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
            };
        }

        public enum State {
            Neutral,
            Failed,
            Checked
        }

        #endregion

        #region Image

        [UIComponent("image-component"), UsedImplicitly] private ImageView _imageComponent;

        private void InitializeMaterial() {
            _imageComponent.material = BundleLoader.UIAdditiveGlowMaterial;
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