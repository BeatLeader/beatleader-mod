﻿using System;
using BeatLeader.Models;
using BeatLeader.Utils;
using HarmonyLib;
using JetBrains.Annotations;
using BGLib.Polyglot;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.DataManager {
    internal class ModifiersManager : MonoBehaviour {
        #region Start / OnDestroy

        [Inject, UsedImplicitly] private GameplaySetupViewController _gameplayController;
        private GameplayModifiersPanelController _modifiersController;

        private enum State { Default, Overriden }
        private State _currentState = State.Default;
        private State _targetState = State.Default;

        private void Start() {
            _modifiersController = (GameplayModifiersPanelController)AccessTools
                .Field(typeof(GameplaySetupViewController), "_gameplayModifiersPanelController").GetValue(_gameplayController);

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
            UpdateModifiersMap(LeaderboardState.IsAnyBeatmapSelected, LeaderboardState.SelectedLeaderboardKey);
            UpdateToggles();
        }

        private void OnSelectedBeatmapWasChanged(bool selectedAny, LeaderboardKey leaderboardKey, BeatmapKey key, BeatmapLevel level) {
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

            var toggles = (GameplayModifierToggle[])AccessTools
                .Field(typeof(GameplayModifiersPanelController), "_gameplayModifierToggles").GetValue(_modifiersController);

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

        private void ApplyDefaultState(GameplayModifierToggle[] toggles) {
            if (_currentState == State.Default) return;
            _currentState = State.Default;

            foreach (var toggle in toggles) {
                toggle.Start(); // return toggle view to default
            }

            GameplayModifiersPanelPatch.isPatchRequired = false;
            RefreshPanel();
        }

        #endregion

        #region OverridenState

        private ModifiersMap _modifiersMap;
        private ModifiersRating? _modifiersRating;
        private int _status = 0;
        private bool _modifiersAvailable;

        private void UpdateModifiersMap(bool isAnyBeatmapSelected, LeaderboardKey leaderboardKey) {
            if (!isAnyBeatmapSelected || !LeaderboardsCache.TryGetLeaderboardInfo(leaderboardKey, out var data)) {
                GameplayModifiersPanelPatch.ModifiersMap = _modifiersMap = default;
                GameplayModifiersPanelPatch.hasModifiers = _modifiersAvailable = false;
                _status = 0;
                _modifiersRating = default;
                return;
            }

            GameplayModifiersPanelPatch.ModifiersMap = _modifiersMap = data.DifficultyInfo.modifierValues;
            GameplayModifiersPanelPatch.ModifiersRating =_modifiersRating = data.DifficultyInfo.modifiersRating;
            GameplayModifiersPanelPatch.hasModifiers = _modifiersAvailable = true;
            _status = data.DifficultyInfo.status;
        }

        private void ApplyOverridenState(GameplayModifierToggle[] toggles) {
            _currentState = State.Overriden;

            foreach (var toggle in toggles) {
                var multiplierText = (TextMeshProUGUI)AccessTools.Field(toggle.GetType(), "_multiplierText").GetValue(toggle);
                var modCode = ModifiersUtils.ToNameCode(toggle.gameplayModifier.modifierNameLocalizationKey);
                if (_modifiersRating is not null && modCode is "SS" or "FS" or "SF") {
                    var stars = modCode switch {
                        "SS" => _modifiersRating.ssStars,
                        "FS" => _modifiersRating.fsStars,
                        "SF" => _modifiersRating.sfStars,
                        _ => 0
                    };
                    multiplierText.text = $"<color=yellow>★ {stars:F2}</color>";
                    continue;
                }
                if (modCode is "NF") {
                    if (_status == 3 || _status == 6) {
                        multiplierText.text = "0% / 0pp";
                    } else {
                        toggle.Start();
                    }


                    continue;
                }
                var multiplierValue = _modifiersAvailable ? _modifiersMap.GetMultiplier(modCode) : 0.0f;
                multiplierText.text = multiplierValue != 0.0f ? FormatToggleText(multiplierValue, toggle.gameplayModifier.multiplierConditionallyValid) : "";
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
            if (_modifiersController.gameObject.activeInHierarchy) {
                _gameplayController.RefreshActivePanel();
            }
        }

        #endregion
    }
}