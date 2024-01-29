using UnityEngine;

namespace BeatLeader.Components {
    internal class AlphaSlider : SliderComponentBase<AlphaSlider> {
        protected override Material GetMaterialPrefab() {
            return BundleLoader.SliderMaterials.alpha;
        }
    }
}