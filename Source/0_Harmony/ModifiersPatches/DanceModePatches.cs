using System.Collections.Generic;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader {
    [HarmonyPatch(typeof(GameEnergyCounter), "HandleNoteWasCut")]
    internal static class DanceEnergyChargePatch {
        [UsedImplicitly]
        private static bool Prefix(NoteController noteController, in NoteCutInfo noteCutInfo) {
            return !(DanceModeTracker.IsDraining
                && noteCutInfo.allIsOK
                && noteController.noteData.gameplayType != NoteData.GameplayType.Bomb);
        }
    }

    [HarmonyPatch(typeof(ScoreUIController), "UpdateScore", typeof(int))]
    internal static class DanceScoreTextPatch {
        [UsedImplicitly]
        private static bool Prefix() {
            return !DanceModeTracker.IsRunning;
        }
    }

    [HarmonyPatch(typeof(LightWithIdManager), nameof(LightWithIdManager.SetColorForId))]
    internal static class DanceDreadLights {
        private static readonly Color bloodRed = new(0.9f, 0.04f, 0.03f);
        private static readonly Dictionary<LightWithIdManager, Color?[]> rawColors = new();
        private static readonly Dictionary<TubeBloomPrePassLight, Color> staticLights = new();

        private static float _dim;
        private static float _tint;
        private static float _desaturation;
        private static bool _refreshing;
        private static bool _staticLightsCollected;

        public static void Apply(float dim, float tint, float desaturation) {
            if (Mathf.Abs(dim - _dim) < 0.004f && Mathf.Abs(tint - _tint) < 0.004f && Mathf.Abs(desaturation - _desaturation) < 0.004f) return;
            _dim = dim;
            _tint = tint;
            _desaturation = desaturation;
            Refresh();
        }

        public static void Reset() {
            _dim = 0f;
            _tint = 0f;
            _desaturation = 0f;
            Refresh();
            rawColors.Clear();
            staticLights.Clear();
            _staticLightsCollected = false;
        }

        private static void CollectStaticLights() {
            _staticLightsCollected = true;
            var driven = new HashSet<TubeBloomPrePassLight>();
            foreach (var withId in Object.FindObjectsByType<TubeBloomPrePassLightWithId>(FindObjectsSortMode.None)) {
                if (withId._tubeBloomPrePassLight != null) driven.Add(withId._tubeBloomPrePassLight);
            }
            foreach (var light in Object.FindObjectsByType<TubeBloomPrePassLight>(FindObjectsSortMode.None)) {
                if (!driven.Contains(light)) staticLights[light] = light.color;
            }
        }

        private static void Refresh() {
            if (!_staticLightsCollected && DanceDreadController.IsHaunting) CollectStaticLights();
            foreach (var pair in staticLights) {
                if (pair.Key != null) pair.Key.color = Recolor(pair.Value);
            }
            _refreshing = true;
            try {
                foreach (var pair in rawColors) {
                    if (pair.Key == null) continue;
                    var colors = pair.Value;
                    for (var id = 0; id < colors.Length; id++) {
                        if (colors[id] is { } color) pair.Key.SetColorForId(id, color);
                    }
                }
            } finally {
                _refreshing = false;
            }
        }

        [UsedImplicitly]
        private static void Prefix(LightWithIdManager __instance, int lightId, ref Color color) {
            if (!DanceDreadController.IsHaunting) return;
            if (!_refreshing) {
                if (!rawColors.TryGetValue(__instance, out var colors)) {
                    colors = new Color?[LightWithIdManager.kMaxLightId + 1];
                    rawColors[__instance] = colors;
                }
                colors[lightId] = color;
            }
            color = Recolor(color);
        }

        private static Color Recolor(Color color) {
            if (_dim <= 0f && _tint <= 0f && _desaturation <= 0f) return color;
            var gray = color.r * 0.3f + color.g * 0.59f + color.b * 0.11f;
            var faded = Color.Lerp(color, new Color(gray, gray, gray), _desaturation);
            var brightness = Mathf.Max(faded.r, Mathf.Max(faded.g, faded.b));
            var tinted = Color.Lerp(faded, bloodRed * brightness, _tint) * (1f - _dim);
            return new Color(tinted.r, tinted.g, tinted.b, color.a);
        }
    }

    [HarmonyPatch(typeof(SaberTrailRenderer), "UpdateVertices")]
    internal static class DanceDreadTrailPatch {
        [UsedImplicitly]
        private static void Postfix(SaberTrailRenderer __instance) {
            var droop = DanceDreadController.TrailDroop;
            if (droop <= 0f || IsGhostTrail(__instance)) return;
            var vertices = __instance._vertices;
            var uvs = __instance._uvs;
            for (var i = 0; i < vertices.Length; i++) {
                var age = uvs[i].y;
                vertices[i].y -= droop * age * age;
            }
        }

        private static bool IsGhostTrail(SaberTrailRenderer renderer) {
            return renderer.name.StartsWith("BattleRoyale");
        }
    }

    [HarmonyPatch(typeof(ColorManager), nameof(ColorManager.ColorForType), typeof(ColorType))]
    internal static class DanceDreadNoteColorPatch {
        [UsedImplicitly]
        private static void Postfix(ref Color __result) {
            var gray = DanceDreadController.NoteGray;
            var dim = DanceDreadController.NoteDim;
            if (gray <= 0f && dim <= 0f) return;
            var luminance = 0.299f * __result.r + 0.587f * __result.g + 0.114f * __result.b;
            var faded = Color.Lerp(__result, new Color(luminance, luminance, luminance), gray) * (1f - dim);
            __result = new Color(faded.r, faded.g, faded.b, __result.a);
        }
    }

    [HarmonyPatch(typeof(LevelFailedTextEffect), nameof(LevelFailedTextEffect.ShowEffect))]
    internal static class DanceDreadFailTextPatch {
        [UsedImplicitly]
        private static void Postfix(LevelFailedTextEffect __instance) {
            if (!DanceDreadController.IsHaunting) return;
            var failedText = __instance.GetComponentInChildren<LevelFailedText>(true);
            if (failedText == null) return;
            failedText._text.richText = true;
            failedText.OverrideText("YOU DIED\n<size=40%>dance next time</size>");
        }
    }
}
