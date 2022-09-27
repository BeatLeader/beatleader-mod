using UnityEngine;

namespace BeatLeader.Themes {
    [CreateAssetMenu(fileName = "ThemeMaterials", menuName = "ThemeMaterials collection")]
    public class ThemeMaterials : ScriptableObject {
        public Material tier1AvatarFull;
        public Material tier1AvatarSmall;

        public Material tier2AvatarFull;
        public Material tier2AvatarSmall;

        public Material tier3AvatarFull;
        public Material tier3AvatarSmall;

        public bool TryGetAvatarMaterial(ThemeTier tier, bool smallVersion, out Material material) {
            switch (tier) {
                case ThemeTier.Tier1:
                    material = smallVersion ? tier1AvatarSmall : tier1AvatarFull;
                    return true;
                case ThemeTier.Tier2:
                    material = smallVersion ? tier2AvatarSmall : tier2AvatarFull;
                    return true;
                case ThemeTier.Tier3:
                    material = smallVersion ? tier3AvatarSmall : tier3AvatarFull;
                    return true;
                case ThemeTier.Unknown:
                default:
                    material = null;
                    return false;
            }
        }
    }
}