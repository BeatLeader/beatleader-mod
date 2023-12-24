using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class AdvancedImage : LayoutComponentBase<AdvancedImage> {
        #region UI Properties

        [ExternalProperty, UsedImplicitly]
        public Sprite? Sprite {
            get => ImageView.sprite;
            set => ImageView.sprite = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Color Color {
            get => ImageView.color;
            set => ImageView.color = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Material? Material {
            get => ImageView.material;
            set => ImageView.material = value;
        }

        [ExternalProperty, UsedImplicitly]
        public bool PreserveAspect {
            get => ImageView.preserveAspect;
            set => ImageView.preserveAspect = value;
        }

        [ExternalProperty, UsedImplicitly]
        public Image.Type ImageType {
            get => ImageView.type;
            set => ImageView.type = value;
        }

        [ExternalProperty, UsedImplicitly]
        public float PixelsPerUnit {
            get => ImageView.pixelsPerUnitMultiplier;
            set {
                ImageType = Image.Type.Sliced;
                ImageView.pixelsPerUnitMultiplier = value;
            }
        }

        #endregion

        #region UI Components

        [ExternalComponent, UsedImplicitly]
        public ImageView ImageView { get; private set; } = null!;

        #endregion

        #region Setup

        protected override void OnConstruct(Transform parent) {
            var imageGo = parent.gameObject.CreateChild("Image");
            ImageView = imageGo.AddComponent<AdvancedImageView>();
            imageGo.AddComponent<VerticalLayoutGroup>();
            Material = Utilities.ImageResources.NoGlowMat;
            PreserveAspect = true;
            ApplyBSMLRoot(imageGo);
        }
        
        #endregion
    }
}