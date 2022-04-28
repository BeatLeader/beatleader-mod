using System.Collections;
using System.Collections.Generic;
using BeatLeader.Utils;
using HarmonyLib;
using Polyglot;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.DataManager {
    internal class ModifiersManager : MonoBehaviour {

        [Inject] private GameplaySetupViewController _gameplayController;
        private GameplayModifiersPanelController _modifiersController;

        private readonly string _positiveColor = "#00FF77";
        private readonly string _multiplierColor = "#00FFFF";

        private enum State { Default, Overriden }
        private State _state = State.Default;

        private void Start() {
            LeaderboardState.IsVisibleChangedEvent += LeaderboardVisibilityChanged;
            StartCoroutine(UpdateModifiersIfNeeded());

            GameplayModifiersPanelPatch.isPatchRequired = false;

            _modifiersController = (GameplayModifiersPanelController)AccessTools
                .Field(typeof(GameplaySetupViewController), "_gameplayModifiersPanelController").GetValue(_gameplayController);
        }

        private void OnDestroy() {
            LeaderboardState.IsVisibleChangedEvent -= LeaderboardVisibilityChanged;
        }

        private void LeaderboardVisibilityChanged(bool visible) {
            if (!ModifiersUtils.instance.HasModifiers) { return; }
            State requiredState = visible ? State.Overriden : State.Default;
            if (_state == requiredState) { return; }

            if (_gameplayController.gameObject.activeInHierarchy) {
                var toggles = (GameplayModifierToggle[])AccessTools
                    .Field(typeof(GameplayModifiersPanelController), "_gameplayModifierToggles").GetValue(_modifiersController);

                // return default
                if (_state == State.Overriden) {
                    foreach (var toggle in toggles) {
                        toggle.Start(); // return toggle view to default
                    }
                    _state = State.Default;
                    GameplayModifiersPanelPatch.isPatchRequired = false;
                } else {
                    foreach (var toggle in toggles) {
                        var modName = toggle.gameplayModifier.modifierNameLocalizationKey;
                        if (ModifiersUtils.instance.Modifiers.ContainsKey(ModifiersUtils.ToNameCode(modName))) {
                            TextMeshProUGUI multiplierText = (TextMeshProUGUI)AccessTools.Field(toggle.GetType(), "_multiplierText").GetValue(toggle);
                            multiplierText.text = FormatToggleText(toggle);
                        }
                    }
                    _state = State.Overriden;
                    GameplayModifiersPanelPatch.isPatchRequired = true;
                }

                if (_modifiersController.gameObject.activeInHierarchy) {
                    _gameplayController.RefreshActivePanel();
                }
            }
        }

        private string FormatToggleText(GameplayModifierToggle toggle) {
            float multiplier = ModifiersUtils.instance.Modifiers[ModifiersUtils.ToNameCode(toggle.gameplayModifier.modifierNameLocalizationKey)];
            bool isPositive = multiplier > 0f;
            string text = isPositive ? string.Format(Localization.Instance.SelectedCultureInfo, "+{0:P0}", multiplier) : string.Format(Localization.Instance.SelectedCultureInfo, "{0:P0}", multiplier);
            if (toggle.gameplayModifier.multiplierConditionallyValid) {
                text = string.Format(Localization.Instance.SelectedCultureInfo, "+{0:P0} / {1}", 0, text);
            }
            return $"<color={(isPositive ? _positiveColor : _multiplierColor)}>{text}</color>";
        }

        private IEnumerator UpdateModifiersIfNeeded() {
            if (ModifiersUtils.instance.Modifiers != null) yield break;
            yield return HttpUtils.GetData<Dictionary<string, float>>(BLConstants.MODIFIERS_URL,
                modifiers => {
                    ModifiersUtils.instance.Modifiers = modifiers;
                },
                reason => Plugin.Log.Error($"Can't fetch values for modifiers. Reason: {reason}"),
                3);
        }
    }
}
