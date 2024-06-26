using HMUI;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class Label : ReactiveComponent, ISkewedComponent {
        public string Text {
            get => _text.text;
            set {
                _text.text = value;
                NotifyPropertyChanged();
            }
        }

        public bool RichText {
            get => _text.richText;
            set {
                _text.richText = value;
                NotifyPropertyChanged();
            }
        }

        public float FontSize {
            get => _text.fontSize;
            set {
                _text.fontSize = value;
                NotifyPropertyChanged();
            }
        }

        public float FontSizeMin {
            get => _text.fontSizeMin;
            set {
                _text.fontSizeMin = value;
                NotifyPropertyChanged();
            }
        }

        public float FontSizeMax {
            get => _text.fontSizeMax;
            set {
                _text.fontSizeMax = value;
                NotifyPropertyChanged();
            }
        }

        public bool EnableAutoSizing {
            get => _text.enableAutoSizing;
            set {
                _text.enableAutoSizing = value;
                NotifyPropertyChanged();
            }
        }

        public FontStyles FontStyle {
            get => _text.fontStyle;
            set {
                _text.fontStyle = value;
                NotifyPropertyChanged();
            }
        }

        public TMP_FontAsset Font {
            get => _text.font;
            set {
                _text.font = value;
                NotifyPropertyChanged();
            }
        }

        public Material Material {
            get => _text.material;
            set {
                _text.material = value;
                NotifyPropertyChanged();
            }
        }

        public bool EnableWrapping {
            get => _text.enableWordWrapping;
            set {
                _text.enableWordWrapping = value;
                NotifyPropertyChanged();
            }
        }

        public TextOverflowModes Overflow {
            get => _text.overflowMode;
            set {
                _text.overflowMode = value;
                NotifyPropertyChanged();
            }
        }

        public TextAlignmentOptions Alignment {
            get => _text.alignment;
            set {
                _text.alignment = value;
                NotifyPropertyChanged();
            }
        }

        public Color Color {
            get => _text.color;
            set {
                _text.color = value;
                NotifyPropertyChanged();
            }
        }

        public bool RaycastTarget {
            get => _text.raycastTarget;
            set {
                _text.raycastTarget = value;
                NotifyPropertyChanged();
            }
        }

        public float Skew {
            get => FontStyle.HasFlag(FontStyles.Italic) ? 1f : 0f;
            set {
                if (value > 0f) {
                    FontStyle |= FontStyles.Italic;
                } else {
                    FontStyle &= ~FontStyles.Italic;
                }
            }
        }

        protected override float? DesiredHeight => _text.preferredHeight;
        protected override float? DesiredWidth => _text.preferredWidth;

        private TextMeshProUGUI _text = null!;

        protected override void Construct(RectTransform rect) {
            _text = rect.gameObject.AddComponent<CurvedTextMeshPro>();
            _text.RegisterDirtyLayoutCallback(RefreshLayout);
            _text.fontSharedMaterial = GameResources.UIFontMaterial;
        }

        protected override void OnInitialize() {
            FontSize = 4f;
            Alignment = TextAlignmentOptions.Center;
            EnableWrapping = false;
        }

        protected override void OnStart() {
            RefreshLayout();
        }
    }
}