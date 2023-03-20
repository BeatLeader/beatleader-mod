using UnityEngine;

namespace BeatLeader.Themes {
    [CreateAssetMenu(fileName = "ThemesCollection", menuName = "ThemesCollection")]
    public class ThemesCollection : ScriptableObject {
        public ThemeMaterials theSun;
        public ThemeMaterials theMoon;
        public ThemeMaterials theStar;
        public ThemeMaterials sparks;
        public ThemeMaterials special;

        public bool TryGetThemeMaterials(ThemeType themeType, out ThemeMaterials themeMaterials) {
            switch (themeType) {
                case ThemeType.TheSun:
                    themeMaterials = theSun;
                    return true;
                case ThemeType.TheMoon:
                    themeMaterials = theMoon;
                    return true;
                case ThemeType.TheStar:
                    themeMaterials = theStar;
                    return true;
                case ThemeType.Sparks:
                    themeMaterials = sparks;
                    return true;
                case ThemeType.Special:
                    themeMaterials = special;
                    return true;
                case ThemeType.Unknown:
                default:
                    themeMaterials = null;
                    return false;
            }
        }
    }
}