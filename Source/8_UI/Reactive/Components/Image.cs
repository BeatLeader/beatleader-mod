using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class Image : ReactiveComponent {
        public Sprite? Sprite {
            get => _image.sprite;
            set => _image.sprite = value;
        }
        
        public Color Color {
            get => _image.color;
            set => _image.color = value;
        }

        public Color GradientColor0 {
            get => _image.color0;
            set => _image.color0 = value;
        }
        
        public Color GradientColor1 {
            get => _image.color1;
            set => _image.color1 = value;
        }

        public bool UseGradient {
            get => _image.gradient;
            set => _image.gradient = value;
        }

        public ImageView.GradientDirection GradientDirection {
            get => _image.GradientDirection;
            set => _image.GradientDirection = value;
        }
        
        public Material? Material {
            get => _image.material;
            set => _image.material = value;
        }
        
        public bool PreserveAspect {
            get => _image.preserveAspect;
            set => _image.preserveAspect = value;
        }
        
        public UnityEngine.UI.Image.Type ImageType {
            get => _image.type;
            set => _image.type = value;
        }
        
        public float PixelsPerUnit {
            get => _image.pixelsPerUnitMultiplier;
            set {
                ImageType = UnityEngine.UI.Image.Type.Sliced;
                _image.pixelsPerUnitMultiplier = value;
            }
        }

        private FixedImageView _image = null!;
        
        protected override void Construct(RectTransform rect) {
            _image = rect.gameObject.AddComponent<FixedImageView>();
            Material = GameResources.UINoGlowMaterial;
        }
    }
}