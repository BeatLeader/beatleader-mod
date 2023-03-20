﻿using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader {
    public static class ModifiersMapManager {
        static ModifiersMapManager() {
            _gameplayModifiersParams = Resources.FindObjectsOfTypeAll<GameplayModifierParamsSO>();
            _gameplayModifiersMap.LoadFromGameModifiersParams(_gameplayModifiersParams);
        }

        public static ModifiersMap CurrentModifiersMap { get; private set; }

        private static readonly GameplayModifierParamsSO[] _gameplayModifiersParams;
        private static readonly ModifiersMap _gameplayModifiersMap;

        public static void LoadCustomModifiersMap(ModifiersMap map, Func<float, float>? applier = null) {
            LoadModifiersParams(_gameplayModifiersParams, map, applier);
        }
        public static void LoadGameplayModifiersMap() {
            LoadModifiersParams(_gameplayModifiersParams, _gameplayModifiersMap);
        }

        private static void LoadModifiersParams(
            IEnumerable<GameplayModifierParamsSO> modifiersParams,
            ModifiersMap map, Func<float, float>? applier = null) {
            foreach (var item in modifiersParams) {
                var modifierServerName = item.modifierNameLocalizationKey.ParseModifierLocalizationKeyToServerName();
                var multiplier = map.GetModifierValueByModifierServerName(modifierServerName);
                if (multiplier is -1) continue;

                multiplier = applier?.Invoke(multiplier) ?? multiplier;
                item.SetField("_multiplier", multiplier);
            }
            CurrentModifiersMap = map;
        }
    }
}
