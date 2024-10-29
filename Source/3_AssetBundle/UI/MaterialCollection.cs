using UnityEngine;

#nullable disable

namespace BeatLeader {
    [CreateAssetMenu(fileName = "MaterialCollection", menuName = "MaterialCollection")]
    public class MaterialCollection : ScriptableObject {
        public Material blurMaterial;
        public Material tintedBlurredBackgroundMaterial;
        public Material blurredBackgroundMaterial;
        public Material uiAdditiveGlowMaterial;
        public Material uiNoDepthMaterial;
    }
}