using UnityEngine;

namespace BeatLeader.Components {
    internal class NormalizedSlider : SliderComponentBase<NormalizedSlider> {
        protected override Material GetMaterialPrefab() {
            return BundleLoader.SliderMaterials.normalized;
        }
    }
}