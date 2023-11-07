using BeatSaberMarkupLanguage;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class AdvancedImage : LayoutComponentBase<AdvancedImage> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public Sprite? Sprite {
            get => Image.sprite;
            set => Image.sprite = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Color Color {
            get => Image.color;
            set => Image.color = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Material? Material {
            get => Image.material;
            set => Image.material = value;
        }

        [ExternalProperty, UsedImplicitly]
        public bool PreserveAspect {
            get => Image.preserveAspect;
            set => Image.preserveAspect = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Image.Type ImageType {
            get => Image.type;
            set => Image.type = value;
        }

        [ExternalProperty, UsedImplicitly]
        public float PixelsPerUnit {
            get => Image.pixelsPerUnitMultiplier;
            set {
                ImageType = Image.Type.Sliced;
                Image.pixelsPerUnitMultiplier = value;
            }
        }

        #endregion

        #region UI Components

        [ExternalComponent, UsedImplicitly]
        public Image Image { get; private set; } = null!;

        #endregion

        #region Setup

        protected override void OnInitialize() {
            Image = Content!.AddComponent<AdvancedImageView>();
            Material = Utilities.ImageResources.NoGlowMat;
            PreserveAspect = true;
        }

        #endregion
    }
}