using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.MainPanel.ScoreRowDivider.bsml")]
    internal class ScoreRowDivider : ReeUIComponent {
        #region OnInitialize

        protected override void OnInitialize() {
            ApplyAlpha();
        }

        #endregion

        #region Animation

        private const float FadeSpeed = 12.0f;
        private float _currentAlpha;
        private float _targetAlpha;

        public void FadeIn() {
            _targetAlpha = 1.0f;
        }

        public void FadeOut() {
            _targetAlpha = 0.0f;
        }

        private void LateUpdate() {
            var t = Time.deltaTime * FadeSpeed;
            LerpAlpha(t);
        }

        private void LerpAlpha(float t) {
            if (_currentAlpha.Equals(_targetAlpha)) return;
            _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, t);
            ApplyAlpha();
        }

        private void ApplyAlpha() {
            _dividerIcon.color = _dividerIcon.color.ColorWithAlpha(_currentAlpha);
        }

        #endregion

        #region Icon

        [UIComponent("divider-icon"), UsedImplicitly]
        private ImageView _dividerIcon;

        #endregion
    }
}