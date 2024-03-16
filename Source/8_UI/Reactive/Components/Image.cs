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