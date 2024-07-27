using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.Themes {
    internal static class ThemesUtils {
        #region GetAvatarParams

        public static void GetAvatarParams(
            IPlayerProfileSettings? profileSettings, bool useSmallMaterialVersion,
            out Material baseMaterial, out float hueShift, out float saturation
        ) {
            if (profileSettings == null) {
                hueShift = 0.0f;
                saturation = 1.0f;
                baseMaterial = BundleLoader.DefaultAvatarMaterial;
                return;
            }

            hueShift = (profileSettings.EffectHue / 360.0f) * (Mathf.PI * 2);
            saturation = profileSettings.EffectSaturation;
            baseMaterial = GetAvatarMaterial(profileSettings.ThemeType, profileSettings.ThemeTier, useSmallMaterialVersion);
        }

        #endregion

        #region GetAvatarMaterial

        public static Material GetAvatarMaterial(ThemeType themeType, ThemeTier themeTier,bool smallVersion) {
            if (themeType is ThemeType.Unknown || themeTier is ThemeTier.Unknown) return BundleLoader.DefaultAvatarMaterial;
            if (!BundleLoader.ThemesCollection.TryGetThemeMaterials(themeType, out var themeMaterials)) return BundleLoader.DefaultAvatarMaterial;
            return !themeMaterials.TryGetAvatarMaterial(themeTier, smallVersion, out var material) ? BundleLoader.DefaultAvatarMaterial : material;
        }

        #endregion

        #region ParseEffectName

        public static void ParseEffectName(string effectName, out ThemeType themeType, out ThemeTier themeTier) {
            var split = effectName.Split('_');

            if (split.Length < 2) {
                themeType = ThemeType.Unknown;
                themeTier = ThemeTier.Unknown;
                return;
            }

            themeType = split[0] switch {
                "Booster" => ThemeType.Booster,
                "TheSun" => ThemeType.TheSun,
                "TheMoon" => ThemeType.TheMoon,
                "TheStar" => ThemeType.TheStar,
                "Sparks" => ThemeType.Sparks,
                "Special" => ThemeType.Special,
                _ => ThemeType.Unknown
            };

            themeTier = split[1] switch {
                "Tier1" => ThemeTier.Tier1,
                "Tier2" => ThemeTier.Tier2,
                "Tier3" => ThemeTier.Tier3,
                _ => ThemeTier.Unknown
            };
        }

        #endregion
    }
}