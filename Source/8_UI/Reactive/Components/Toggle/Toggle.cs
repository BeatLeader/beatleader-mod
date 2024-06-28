using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class Toggle : ReactiveComponent {
        #region Props

        public bool Active {
            get => _active;
            set {
                _active = value;
                if (value) {
                    _animator.Push();
                } else {
                    _animator.Pull();
                }
                NotifyPropertyChanged();
            }
        }

        public bool Interactable {
            get => _interactable;
            set {
                _interactable = value;
                _backgroundButton.Interactable = value;
                NotifyPropertyChanged();
            }
        }

        private bool _active;
        private bool _interactable = true;
        
        public void SetActive(bool active, bool animated = true, bool silent = false) {
            _active = active;
            if (active) {
                _animator.Push();
            } else {
                _animator.Pull();
            }
            if (!animated) {
                _animator.SetProgress(active ? 1f : 0f);
            }
            if (!silent) {
                NotifyPropertyChanged(nameof(Active));
            }
        }

        #endregion

        #region Setup

        private readonly ValueAnimator _animator = new();

        protected override void OnUpdate() {
            _animator.Update();
            var progress = _animator.Progress;
            LerpPosition(progress);
            LerpStretch(progress);
            LerpColor(progress);
            LerpText(progress);
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { x = 18f, y = 6f });
        }

        #endregion

        #region Animation

        private float _knobMargin = 1f;
        private float _knobWidth = 7.55f;
        private float _knobHeight = 5f;
        private float _horizontalStretchAmount = 0.8f;
        private float _verticalStretchAmount = 0.8f;

        private void LerpText(float switchAmount) {
            _onLabel.Color = Color.Lerp(
                Color.clear,
                UIStyle.TextColorSet.Color,
                switchAmount
            );
            _offLabel.Color = Color.Lerp(
                Color.clear,
                UIStyle.TextColorSet.DisabledColor,
                1f - switchAmount
            );
        }

        private void LerpColor(float switchAmount) {
            var color = Color.Lerp(
                UIStyle.ControlColorSet.Color,
                UIStyle.ControlButtonColorSet.ActiveColor,
                switchAmount
            );
            if (!_interactable) color.a -= 0.3f;
            _knobImage.Color = color;
        }

        private void LerpPosition(float switchAmount) {
            var min = _knobTransform.anchorMin;
            min.x = switchAmount;
            _knobTransform.anchorMin = min;
            var max = _knobTransform.anchorMax;
            max.x = switchAmount;
            _knobTransform.anchorMax = max;
        }

        private void LerpStretch(float switchAmount) {
            var factor = 1f - Mathf.Abs(switchAmount - 0.5f) * 2f;
            var x = _knobWidth * (1f + _horizontalStretchAmount * factor);
            var y = _knobHeight * (_verticalStretchAmount * -factor) - _knobMargin;
            _knobTransform.sizeDelta = new(x, y);
        }

        #endregion

        #region Construct

        private Image _knobImage = null!;
        private RectTransform _knobTransform = null!;
        private Label _onLabel = null!;
        private Label _offLabel = null!;
        private ImageButton _backgroundButton = null!;

        protected override GameObject Construct() {
            return new ImageButton {
                Image = {
                    Sprite = BundleLoader.Sprites.background,
                    PixelsPerUnit = 12f,
                    Material = GameResources.UINoGlowMaterial
                },
                Colors = UIStyle.ControlButtonColorSet,
                GrowOnHover = false,
                HoverLerpMul = float.MaxValue,
                Children = {
                    //text area
                    new Dummy {
                        Children = {
                            new Label {
                                Text = "I",
                                Alignment = TextAlignmentOptions.Center
                            }.AsFlexItem(size: new() { x = "50%" }).Bind(ref _onLabel),
                            //
                            new Label {
                                Text = "O",
                                Alignment = TextAlignmentOptions.Center
                            }.AsFlexItem(size: new() { x = "50%" }).Bind(ref _offLabel)
                        }
                    }.AsFlexGroup().WithRectExpand(),
                    //knob slide area
                    new Dummy {
                        Children = {
                            //knob
                            new Image {
                                ContentTransform = {
                                    anchorMin = Vector2.zero,
                                    anchorMax = new(0f, 1f),
                                },
                                Sprite = BundleLoader.Sprites.background,
                                PixelsPerUnit = 12f,
                                Color = Color.cyan
                            }.Bind(ref _knobTransform).Bind(ref _knobImage)
                        }
                    }.WithRectExpand().WithSizeDelta(-_knobWidth - _knobMargin, 0f)
                }
            }.WithClickListener(
                () => {
                    _active = !_active;
                    Active = _active;
                }
            ).Bind(ref _backgroundButton).Use();
        }

        #endregion
    }
}