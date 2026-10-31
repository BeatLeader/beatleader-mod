using System;
using System.Collections.Generic;
using System.Linq;
using ModifiersCore;

namespace BeatLeader {
    public static class NegativeModifiers {
        internal static readonly string[] AlwaysNegative = { "NF", "NO", "NB", "NA", "SS", "SC" };

        private static readonly (string id, string other)[] restrictedModifiers = {
            (DanceModifier.Id, GolfModifier.Id),
            (GolfModifier.Id, DanceModifier.Id)
        };

        private static string[] _negativeModifiers = AlwaysNegative;

        public static void UpdateExclusiveModifiers(IReadOnlyDictionary<string, float>? modifierValues) {
            _negativeModifiers = modifierValues == null
                ? AlwaysNegative
                : modifierValues
                    .Where(x => x.Value < 0)
                    .Select(x => x.Key.ToUpperInvariant())
                    .Where(ModifiersManager.HasModifierWithId)
                    .Union(AlwaysNegative)
                    .ToArray();

            foreach (var (id, other) in restrictedModifiers) {
                if (!ModifiersManager.HasModifierWithId(id)) continue;
                ModifiersManager.SetMutuallyExclusiveModifiers(id, _negativeModifiers.Append(other));
                if (HasNegativeModifiers(id)) {
                    DisableModifier(id);
                }
            }
        }

        public static bool HasNegativeModifiers(string id) {
            return IsEnabled(id) && _negativeModifiers.Any(IsEnabled);
        }

        private static bool IsEnabled(string id) {
            try {
                return ModifiersManager.GetModifierState(id);
            } catch (InvalidOperationException) {
                return false;
            }
        }

        private static void DisableModifier(string id) {
            var toggle = ModifiersManager.Toggles()?.FirstOrDefault(x => x.Item2.Id == id).Item1;
            if (toggle != null) {
                toggle.toggle.isOn = false;
            } else {
                ModifiersManager.SetModifierState(id, false);
            }
        }
    }
}
