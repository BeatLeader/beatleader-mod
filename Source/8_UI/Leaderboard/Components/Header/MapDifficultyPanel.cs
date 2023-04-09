using System;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class MapDifficultyPanel : ReeUIComponentV2 {
        #region Static Events

        private delegate void MapStatusHoverDelegate(Vector3 worldPos, bool isHovered, float progress);

        private static event MapStatusHoverDelegate? OnMapStatusHoverStateChanged;

        public static void NotifyMapStatusHoverStateChanged(Vector3 worldPos, bool isHovered, float progress) {
            OnMapStatusHoverStateChanged?.Invoke(worldPos, isHovered, progress);
        }

        private static event Action<DiffInfo>? OnDiffInfoChanged;

        public static void NotifyDiffInfoChanged(DiffInfo diffInfo) {
            OnDiffInfoChanged?.Invoke(diffInfo);
        }

        #endregion

        #region Components

        [UIComponent("root"), UsedImplicitly]
        private Transform _root;

        [UIValue("skill-triangle"), UsedImplicitly]
        private SkillTriangle _skillTriangle;

        private void Awake() {
            _skillTriangle = Instantiate<SkillTriangle>(transform);
        }

        #endregion

        #region Init / Dispose

        protected override void OnInitialize() {
            OnMapStatusHoverStateChanged += OnHoverStateChanged;
            OnDiffInfoChanged += SetDiffInfo;
            IsActive = false;
        }

        protected override void OnDispose() {
            OnMapStatusHoverStateChanged -= OnHoverStateChanged;
        }

        #endregion

        #region IsActive

        private bool _isActive = true;

        private bool IsActive {
            get => _isActive;
            set {
                if (_isActive == value) return;
                _isActive = value;
                _root.gameObject.SetActive(value);
            }
        }

        #endregion

        #region Events

        private bool _hoverEnabled;

        private void SetDiffInfo(DiffInfo diffInfo) {
            _hoverEnabled = (diffInfo.techRating + diffInfo.accRating + diffInfo.passRating) > 0.0f;
            _skillTriangle.SetValues(diffInfo.techRating, diffInfo.accRating, diffInfo.passRating);
        }

        private void OnHoverStateChanged(Vector3 worldPos, bool isHovered, float progress) {
            if (!_hoverEnabled || progress <= 0.3f) {
                IsActive = false;
                return;
            }

            IsActive = true;

            var scale = Mathf.Pow(progress, isHovered ? 0.5f : 2.0f);
            _root.localScale = new Vector3(0.5f + 0.5f * scale, scale, 1.0f);

            var p = _root.parent.InverseTransformPoint(worldPos);
            _root.localPosition = new Vector3(p.x, p.y - 5.0f, 0.0f);
        }

        #endregion
    }
}