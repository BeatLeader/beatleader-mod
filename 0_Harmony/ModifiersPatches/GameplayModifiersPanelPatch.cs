using System.Collections.Generic;
using BeatLeader.Utils;
using HarmonyLib;
using Polyglot;
using TMPro;
using UnityEngine;

namespace BeatLeader {

    [HarmonyPatch(typeof(GameplayModifiersPanelController), "RefreshTotalMultiplierAndRankUI")]
    public static class GameplayModifiersPanelPatch {

        public static bool isPatchRequired = false;

        private static void Postfix(GameplayModifiersPanelController __instance, Color ____positiveColor, Color ____negativeColor,
                                    GameplayModifiersModelSO ____gameplayModifiersModel, GameplayModifiers ____gameplayModifiers,
                                    TextMeshProUGUI ____totalMultiplierValueText, TextMeshProUGUI ____maxRankValueText) {

            if (ModifiersUtils.instance.HasModifiers && isPatchRequired) {
                List<GameplayModifierParamsSO> modifierParams = ____gameplayModifiersModel.CreateModifierParamsList(____gameplayModifiers);
                float totalMultiplier = 1;

                foreach (var param in modifierParams) {
                    if (!param.multiplierConditionallyValid) { // for now only NoFail being ignored
                        string key = ModifiersUtils.ToNameCode(param.modifierNameLocalizationKey);
                        if (key != null && ModifiersUtils.instance.Modifiers.ContainsKey(key)) {
                            totalMultiplier += ModifiersUtils.instance.Modifiers[key];
                        } else {
                            totalMultiplier += param.multiplier;
                        }
                    }
                }
                if (totalMultiplier < 0) totalMultiplier = 0;  // thanks Beat Games for Zen mode -1000%

                Color color = (totalMultiplier >= 1f) ? ____positiveColor : ____negativeColor;
                ____totalMultiplierValueText.text = string.Format(Localization.Instance.SelectedCultureInfo, "{0:P0}", totalMultiplier);
                ____totalMultiplierValueText.color = color;

                string rank = ModifiersUtils.GetRankForMultiplier(totalMultiplier);
                ____maxRankValueText.text = rank;
                ____maxRankValueText.color = color;
            }
        }
    }
}