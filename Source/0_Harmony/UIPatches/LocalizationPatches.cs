using HarmonyLib;
using JetBrains.Annotations;
using TMPro;

namespace BeatLeader {
    [HarmonyPatch(typeof(MaterialReferenceManager), "TryGetFontAssetInternal")]
    public static class TryGetFontAssetPatch {
        [UsedImplicitly]
        private static void Postfix(int hashCode, ref TMP_FontAsset fontAsset, ref bool __result) {
            if (fontAsset != null) return;
            __result = BundleLoader.TryGetFontAsset(hashCode, ref fontAsset);
        }
    }

    [HarmonyPatch(typeof(TMP_Text), "StringToCharArray")]
    public static class PreProcessTextPatch {
        [UsedImplicitly]
        private static void Prefix(ref string sourceText) {
            if (sourceText == null) return;
            while (true) {
                if (!ProcessLocalizationTag(ref sourceText)) break;
            }
        }

        private static bool ProcessLocalizationTag(ref string sourceText) {
            const string from = "<bll>";
            const string to = "</bll>";

            var openIndexA = FindSubstrIndexUnsafe(sourceText, from, 0);
            if (openIndexA == -1) return false;
            var openIndexB = openIndexA + from.Length;

            var closeIndexA = FindSubstrIndexUnsafe(sourceText, to, openIndexB);
            if (closeIndexA == -1) return false;
            var closeIndexB = closeIndexA + to.Length;

            var token = sourceText.Substring(openIndexB, closeIndexA - openIndexB);
            sourceText = sourceText.Remove(openIndexA, closeIndexB - openIndexA);
            sourceText = sourceText.Insert(openIndexA, BLLocalization.GetTranslationWithFont(token));
            return true;
        }

        private static int FindSubstrIndexUnsafe(string source, string substr, int startIndex) {
            var index = -1;

            for (var i = startIndex; i < source.Length - substr.Length + 1; i++) {
                var matches = true;
                for (var j = 0; j < substr.Length; j++) {
                    if (source[i + j] == substr[j]) continue;
                    matches = false;
                    break;
                }

                if (!matches) continue;
                index = i;
                break;
            }

            return index;
        }
    }
}