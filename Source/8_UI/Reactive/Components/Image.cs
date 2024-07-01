using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class Image : DrivingReactiveComponent, ISkewedComponent {
        public Sprite? Sprite {
            get => _image.sprite;
            set {
                _image.sprite = value;
                NotifyPropertyChanged();
            }
        }

        public Color Color {
            get => _image.color;
            set {
                _image.color = value;
                NotifyPropertyChanged();
            }
        }

        public Color GradientColor0 {
            get => _image.color0;
            set {
                _image.color0 = value;
                NotifyPropertyChanged();
            }
        }

        public Color GradientColor1 {
            get => _image.color1;
            set {
                _image.color1 = value;
                NotifyPropertyChanged();
            }
        }

        public bool UseGradient {
            get => _image.gradient;
            set {
                _image.gradient = value;
                NotifyPropertyChanged();
            }
        }

        public ImageView.GradientDirection GradientDirection {
            get => _image.GradientDirection;
            set {
                _image.GradientDirection = value;
                NotifyPropertyChanged();
            }
        }

        public Material? Material {
            get => _image.material;
            set {
                _image.material = value;
                NotifyPropertyChanged();
            }
        }

        public bool PreserveAspect {
            get => _image.preserveAspect;
            set {
                _image.preserveAspect = value;
                NotifyPropertyChanged();
            }
        }

        public UnityEngine.UI.Image.Type ImageType {
            get => _image.type;
            set {
                _image.type = value;
                NotifyPropertyChanged();
            }
        }

        public float PixelsPerUnit {
            get => _image.pixelsPerUnitMultiplier;
            set {
                ImageType = UnityEngine.UI.Image.Type.Sliced;
                _image.pixelsPerUnitMultiplier = value;
                NotifyPropertyChanged();
            }
        }

        public float Skew {
            get => _image.Skew;
            set => _image.Skew = value;
        }

        private FixedImageView _image = null!;
        
        protected override void Construct(RectTransform rect) {
            _image = rect.gameObject.AddComponent<FixedImageView>();
            Material = GameResources.UINoGlowMaterial;
        }
    }
}