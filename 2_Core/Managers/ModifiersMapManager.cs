using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader
{
    public static class ModifiersMapManager
    {
        static ModifiersMapManager()
        {
            _gameplayModifiersParams = Resources.FindObjectsOfTypeAll<GameplayModifierParamsSO>();
            _gameplayModifiersMap.LoadFromGameModifiersParams(_gameplayModifiersParams);
        }

        private static readonly GameplayModifierParamsSO[] _gameplayModifiersParams;
        private static readonly ModifiersMap _gameplayModifiersMap;
        private static ModifiersMap _customModifiersMap;

        public static void LoadCustomModifiersMap(ModifiersMap map, Func<float, float> applier)
        {
            LoadModifiersParams(_gameplayModifiersParams, _customModifiersMap = map, applier);
        }
        public static void LoadCustomModifiersMap(ModifiersMap map)
        {
            LoadModifiersParams(_gameplayModifiersParams, _customModifiersMap = map);
        }
        public static void LoadGameplayModifiersMap()
        {
            LoadModifiersParams(_gameplayModifiersParams, _gameplayModifiersMap);
        }

        private static void LoadModifiersParams(
            IEnumerable<GameplayModifierParamsSO> modifiersParams, 
            ModifiersMap map, Func<float, float> applier = null)
        {
            foreach (var item in modifiersParams)
            {
                var modifierServerName = item.modifierNameLocalizationKey.ParseModifierLocalizationKeyToServerName();
                var multiplier = map.GetModifierValueByModifierServerName(modifierServerName);
                if (multiplier is -1) continue;

                multiplier = applier?.Invoke(multiplier) ?? multiplier;
                Plugin.Log.Info($"{modifierServerName} - {multiplier}");
                item.SetField("_multiplier", multiplier);
            }
        }
    }
}
