using System;
using BeatLeader.Models;
using BeatLeader.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using Polyglot;
using TMPro;
using UnityEngine;
using Zenject;
using System.Collections.Generic;
using ModifiersCore;

namespace BeatLeader.DataManager {
    internal class ModifiersManager : MonoBehaviour {
        #region Start / OnDestroy

        [Inject, UsedImplicitly] private GameplaySetupViewController _gameplayController;

        private enum State { Default, Overriden }
        private State _currentState = State.Default;
        private State _targetState = State.Default;

        private void Start() {

            GameplayModifiersPanelPatch.isPatchRequired = false;

            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
            LeaderboardState.IsVisibleChangedEvent += OnLeaderboardVisibilityChanged;
            LeaderboardsCache.CacheWasChangedEvent += OnCacheWasChanged;
            OnLeaderboardVisibilityChanged(LeaderboardState.IsVisible);
        }

        private void OnDestroy() {
            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
            LeaderboardState.IsVisibleChangedEvent -= OnLeaderboardVisibilityChanged;
            LeaderboardsCache.CacheWasChangedEvent -= OnCacheWasChanged;
        }

        #endregion

        #region Events

        private void OnCacheWasChanged() {
            UpdateModifiersMap(LeaderboardState.IsAnyBeatmapSelected, LeaderboardState.SelectedBeatmapKey);
            UpdateToggles();
        }

        private void OnSelectedBeatmapWasChanged(bool selectedAny, LeaderboardKey leaderboardKey, IDifficultyBeatmap beatmap) {
            UpdateModifiersMap(selectedAny, leaderboardKey);
            UpdateToggles();
        }

        private void OnLeaderboardVisibilityChanged(bool isVisible) {
            _targetState = isVisible ? State.Overriden : State.Default;
            UpdateToggles();
        }

        #endregion

        #region UpdateToggles

        private void UpdateToggles() {
            if (!_gameplayController.gameObject.activeInHierarchy) return;

            var toggles = ModifiersCore.ModifiersManager.Toggles();

            if (toggles == null) { return; }

            switch (_targetState) {
                case State.Default:
                    ApplyDefaultState(toggles);
                    break;
                case State.Overriden:
                    ApplyOverridenState(toggles);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region DefaultState

        private void ApplyDefaultState((GameplayModifierToggle, IModifier)[] toggles) {
            if (_currentState == State.Default) return;
            _currentState = State.Default;

            foreach (var toggle in toggles) {
                toggle.Item1.Start(); // return toggle view to default
            }

            GameplayModifiersPanelPatch.isPatchRequired = false;
            RefreshPanel();
        }

        #endregion

        #region OverridenState

        private Dictionary<string, float>? _modifiersMap;
        private Dictionary<string, float>? _modifiersRating;
        private bool _modifiersAvailable;

        private void UpdateModifiersMap(bool isAnyBeatmapSelected, LeaderboardKey leaderboardKey) {
            if (!isAnyBeatmapSelected || !LeaderboardsCache.TryGetLeaderboardInfo(leaderboardKey, out var data)) {
                GameplayModifiersPanelPatch.ModifiersMap = _modifiersMap = null;
                GameplayModifiersPanelPatch.hasModifiers = _modifiersAvailable = false;
                _modifiersRating = default;
                return;
            }

            GameplayModifiersPanelPatch.ModifiersMap = _modifiersMap = data.DifficultyInfo.modifierValues;
            GameplayModifiersPanelPatch.ModifiersRating =_modifiersRating = data.DifficultyInfo.modifiersRating;
            GameplayModifiersPanelPatch.hasModifiers = _modifiersAvailable = true;
        }

        private void ApplyOverridenState((GameplayModifierToggle, IModifier)[] toggles) {
            _currentState = State.Overriden;

            foreach (var toggle in toggles) {
                var multiplierText = toggle.Item1._multiplierText;
                var key = toggle.Item2.Id;
                var starsKey = key.ToLower() + "Stars";

                if (_modifiersRating != null && _modifiersRating.ContainsKey(starsKey)) {
                    var stars = _modifiersRating[starsKey];
                    multiplierText.text = $"<color=yellow>★ {stars:F2}</color>";
                    continue;
                }
                if (_modifiersMap != null) {
                    var multiplierValue = _modifiersAvailable && _modifiersMap.ContainsKey(key.ToLower()) ? _modifiersMap[key.ToLower()] : 0.0f;
                    multiplierText.text = multiplierValue != 0.0f ? FormatToggleText(multiplierValue, toggle.Item2.Id == "NF") : "";
                }
            }

            GameplayModifiersPanelPatch.isPatchRequired = true;
            RefreshPanel();
        }

        #endregion

        #region FormatToggleText

        private const string PositiveColor = "#00FF77";
        private const string MultiplierColor = "#00FFFF";

        private static string FormatToggleText(float multiplier, bool multiplierConditionallyValid) {
            bool isPositive = multiplier > 0f;
            string text = isPositive ? string.Format(Localization.Instance.SelectedCultureInfo, "+{0:P0}", multiplier) : string.Format(Localization.Instance.SelectedCultureInfo, "{0:P0}", multiplier);
            if (multiplierConditionallyValid) {
                text = string.Format(Localization.Instance.SelectedCultureInfo, "+{0:P0} / {1}", 0, text);
            }
            return $"<color={(isPositive ? PositiveColor : MultiplierColor)}>{text}</color>";
        }

        #endregion

        #region Utils

        private void RefreshPanel() {
            if (_gameplayController.gameObject.activeInHierarchy) {
                _gameplayController.RefreshActivePanel();
            }
        }

        #endregion
    }
}