using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ImageButton : ColoredButtonComponentBase<ImageButton> {
        #region UI Components

        [ExternalComponent, UsedImplicitly]
        [ExternalProperty(
            prefix: null,
            nameof(AdvancedImage.Sprite),
            nameof(AdvancedImage.PreserveAspect)
        )]
        public AdvancedImage Image { get; private set; } = null!;

        #endregion

        #region Color

        protected override void ApplyColor(Color color, float t) {
            Image.Color = color;
        }

        #endregion

        #region Setup

        protected override void OnContentConstruct(Transform parent) {
            Image = AdvancedImage.Instantiate(parent);
            Image.InheritSize = true;
            Image.Material = BundleLoader.UIAdditiveGlowMaterial;
        }

        #endregion
    }
}