using UnityEngine;

namespace BeatLeader {
    [CreateAssetMenu(fileName = "MaterialCollection", menuName = "MaterialCollection")]
    public class MaterialCollection : ScriptableObject {
        public Material uiBlurMaterial;
        public Material uiNoGlowMaterial;
        public Material uiAdditiveGlowMaterial;
        public Material uiNoDepthMaterial;
    }
}