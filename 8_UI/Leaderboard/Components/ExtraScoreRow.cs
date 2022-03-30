using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.Components.ExtraScoreRow.bsml")]
    internal class ExtraScoreRow : ReeUIComponent {
        #region Components

        [UIValue("score-row"), UsedImplicitly]
        public readonly ScoreRow ScoreRow = Instantiate<ScoreRow>();

        #endregion

        #region Public

        public void SetHierarchyIndex(int value) {
            Root.SetSiblingIndex(value);
        }

        public void SetActive(bool value) {
            IsActive = value;
        }

        public void SetRowPosition(bool onTop) {
            ScoreRow.SetHierarchyIndex(onTop ? 0 : 1);
        }

        public void SetScore(Score score) {
            ScoreRow.SetScore(score);
            FadeIn();
        }

        public void ClearScore() {
            ScoreRow.ClearScore();
            FadeOut();
        }

        #endregion

        #region OnInitialize

        protected override void OnInitialize() {
            ApplyAlpha();
        }

        #endregion

        #region Animation

        private const float FadeSpeed = 12.0f;
        private float _currentAlpha;
        private float _targetAlpha;

        private void FadeIn() {
            _targetAlpha = 1.0f;
        }

        private void FadeOut() {
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

        #region IsActive

        private bool _isActive = true;

        [UIValue("is-active"), UsedImplicitly]
        private bool IsActive {
            get => _isActive;
            set {
                if (_isActive.Equals(value)) return;
                _isActive = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Icon

        [UIComponent("divider-icon"), UsedImplicitly]
        private ImageView _dividerIcon;

        #endregion
    }
}