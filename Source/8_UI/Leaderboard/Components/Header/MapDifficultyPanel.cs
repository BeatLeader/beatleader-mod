using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using ModifiersCore;
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

        [UIComponent("root"), UsedImplicitly] private Transform _root;

        [UIValue("skill-triangle"), UsedImplicitly] private SkillTriangle _skillTriangle;

        private void Awake() {
            _skillTriangle = Instantiate<SkillTriangle>(transform);
        }

        #endregion

        #region Init / Dispose

        protected override void OnInitialize() {
            OnMapStatusHoverStateChanged += OnHoverStateChanged;
            OnDiffInfoChanged += SetDiffInfo;
            GameplayModifiersPanelPatch.ModifiersChangedEvent += OnModifiersChanged;
            IsActive = false;
        }

        protected override void OnDispose() {
            OnMapStatusHoverStateChanged -= OnHoverStateChanged;
            GameplayModifiersPanelPatch.ModifiersChangedEvent -= OnModifiersChanged;
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
        private DiffInfo _diffInfo;

        private void SetDiffInfo(DiffInfo diffInfo) {
            _diffInfo = diffInfo;
            ModifyDiffRating(ref diffInfo);
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

        private void OnModifiersChanged(GameplayModifiers modifiers) {
            _gameplayModifiers = modifiers;
            _modifiersRating = GameplayModifiersPanelPatch.ModifiersRating;
            _modifiersMap = GameplayModifiersPanelPatch.ModifiersMap;
            SetDiffInfo(_diffInfo);
        }

        #endregion

        #region Modifiers

        private GameplayModifiers _gameplayModifiers = new();
        private Dictionary<string, float>? _modifiersRating;
        private Dictionary<string, float>? _modifiersMap;

        private void ModifyDiffRating(ref DiffInfo diffInfo) {
            if (_modifiersRating == null) return;

            foreach (var modifier in ModifiersManager.Modifiers) {
                var lowerId = modifier.Id.ToLower();
                if (_modifiersRating.ContainsKey(lowerId + "Stars") &&
                    ModifiersManager.GetModifierState(modifier.Id)) {
                    diffInfo.passRating = _modifiersRating[lowerId + "PassRating"];
                    diffInfo.accRating = _modifiersRating[lowerId + "AccRating"];
                    diffInfo.techRating = _modifiersRating[lowerId + "TechRating"];
                    diffInfo.stars = _modifiersRating[lowerId + "Stars"];
                    break;
                }
            }

            var summand = CalculateModifiersSummand();
            ApplyMultiplier(ref diffInfo.passRating, summand);
            ApplyMultiplier(ref diffInfo.accRating, summand);
            ApplyMultiplier(ref diffInfo.techRating, summand);
            ApplyMultiplier(ref diffInfo.stars, summand);
        }

        private static void ApplyMultiplier(ref float baseRating, float summand) {
            baseRating *= 1 + summand;
        }

        private float CalculateModifiersSummand() {
            return ModifiersManager
                    .Modifiers
                    .Where(m => ModifiersManager.GetModifierState(m.Id))
                    .Sum(m => _modifiersMap != null && _modifiersMap.ContainsKey(m.Id.ToLower()) ? _modifiersMap[m.Id.ToLower()] : m.Multiplier);
        }

        #endregion
    }
}