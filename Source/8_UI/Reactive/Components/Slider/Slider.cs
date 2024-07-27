using System;
using BeatLeader.Components;
using HMUI;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class Slider : SliderComponentBase {
        #region UI Props

        public bool ShowValueText {
            get => _text.Enabled;
            set => _text.Enabled = value;
        }

        public bool ShowButtons {
            get => _showButtons;
            set {
                _showButtons = value;
                _background.Image.Sprite = value ?
                    BundleLoader.Sprites.rectangle :
                    BundleLoader.Sprites.background;
                _decrementButton.Enabled = value;
                _incrementButton.Enabled = value;
            }
        }

        public bool Interactable {
            get => _interactable;
            set {
                _interactable = value;
                _decrementButton.Interactable = value;
                _incrementButton.Interactable = value;
                _background.Interactable = value;
                _pointerEventsHandler.enabled = value;
                RefreshHandleAndTextColors();
            }
        }

        public Func<float, string>? ValueFormatter {
            get => _valueFormatter;
            set {
                _valueFormatter = value;
                Refresh();
                NotifyPropertyChanged();
            }
        }

        private Func<float, string>? _valueFormatter;
        private bool _showButtons = true;
        private bool _interactable = true;

        #endregion

        #region Input

        private bool CanIncrement => Value < ValueRange.End;
        private bool CanDecrement => Value > ValueRange.Start;

        protected override void Refresh(bool silent = false, bool forceRefreshValue = false) {
            base.Refresh(silent, forceRefreshValue);
            RefreshButtons();
        }

        private void PlaceText(float handlePos) {
            _text.Text = _valueFormatter?.Invoke(Value) ?? $"{Value}";
            var halfPassed = handlePos > MaxHandlePosition / 2f;
            var textSize = (((ILayoutItem)_text).DesiredWidth ?? 0f) / 2f + 1f;
            var textPos = halfPassed ? handlePos - textSize : handlePos + textSize + _handle.rect.width;
            _text.ContentTransform.localPosition = new(textPos, 0f, 0f);
        }

        protected override void PlaceHandle(float pos) {
            base.PlaceHandle(pos);
            PlaceText(pos);
        }

        private void RefreshButtons() {
            _incrementButton.Interactable = CanIncrement && _interactable;
            _decrementButton.Interactable = CanDecrement && _interactable;
        }

        #endregion

        #region Construct

        protected override PointerEventsHandler SlidingAreaEventsHandler => _pointerEventsHandler;
        protected override RectTransform SlidingAreaTransform => _slidingArea;
        protected override RectTransform HandleTransform => _handle;

        private RectTransform _slidingArea = null!;
        private RectTransform _handle = null!;
        private Image _handleImage = null!;
        private ImageButton _background = null!;
        private ButtonBase _incrementButton = null!;
        private ButtonBase _decrementButton = null!;
        private Label _text = null!;
        private PointerEventsHandler _pointerEventsHandler = null!;

        protected override GameObject Construct() {
            static ButtonBase CreateButton(
                bool applyColor1,
                Sprite backgroundSprite,
                float iconRotation
            ) {
                return new ImageButton {
                    Image = {
                        Sprite = backgroundSprite,
                        PixelsPerUnit = 12f,
                        GradientDirection = ImageView.GradientDirection.Horizontal,
                        Material = GameResources.UINoGlowMaterial
                    },
                    GrowOnHover = false,
                    HoverLerpMul = float.MaxValue,
                    Colors = null,
                    Children = {
                        //icon
                        new Image {
                            Sprite = GameResources.Sprites.ArrowIcon,
                            PreserveAspect = true,
                            Color = Color.white.ColorWithAlpha(0.8f),
                            ContentTransform = {
                                localEulerAngles = new(0f, 0f, iconRotation)
                            }
                        }.AsFlexItem(grow: 1f)
                    }
                }.With(
                    x => {
                        var animatedSet = new StateColorSet {
                            HoveredColor = Color.white.ColorWithAlpha(0.3f),
                            Color = UIStyle.InputColorSet.Color
                        };
                        var staticColor = UIStyle.InputColorSet.Color;
                        x.Image.Color = Color.white;
                        if (!applyColor1) {
                            x.GradientColors0 = animatedSet;
                            x.Image.GradientColor1 = staticColor;
                        } else {
                            x.GradientColors1 = animatedSet;
                            x.Image.GradientColor0 = staticColor;
                        }
                    }
                ).AsFlexGroup(padding: 1.5f).AsFlexItem(basis: 6f);
            }

            return new Dummy {
                Children = {
                    //dec button
                    CreateButton(
                        false,
                        BundleLoader.Sprites.backgroundLeft,
                        270f
                    ).WithClickListener(HandleDecrementButtonClicked).Bind(ref _decrementButton),
                    //sliding area bg
                    new ImageButton {
                        Image = {
                            Sprite = BundleLoader.Sprites.rectangle,
                            PixelsPerUnit = 12f,
                            Material = GameResources.UINoGlowMaterial
                        },
                        GrowOnHover = false,
                        HoverLerpMul = float.MaxValue,
                        Colors = UIStyle.InputColorSet,
                        Children = {
                            //sliding area
                            new Dummy {
                                ContentTransform = {
                                    pivot = new(0f, 0.5f)
                                },
                                Children = {
                                    //text
                                    new Label {
                                        ContentTransform = {
                                            anchorMin = new(0.5f, 0f),
                                            anchorMax = new(0.5f, 1f),
                                            sizeDelta = Vector2.zero
                                        }
                                    }.Bind(ref _text),
                                    //handle
                                    new Image {
                                        ContentTransform = {
                                            anchorMin = new(0.5f, 0f),
                                            anchorMax = new(0.5f, 1f),
                                            sizeDelta = new(1f, 0f),
                                            pivot = new(0f, 0.5f)
                                        },
                                        Sprite = BundleLoader.Sprites.background,
                                        PixelsPerUnit = 30f
                                    }.Bind(ref _handle).Bind(ref _handleImage)
                                }
                            }.AsFlexItem(grow: 1f).Bind(ref _slidingArea),
                        }
                    }.WithNativeComponent(out _pointerEventsHandler).AsFlexGroup(
                        padding: 1f
                    ).AsFlexItem(
                        grow: 1f,
                        margin: new() { left = 0.5f, right = 0.5f }
                    ).Bind(ref _background),
                    //inc button
                    CreateButton(
                        true,
                        BundleLoader.Sprites.backgroundRight,
                        90f
                    ).WithClickListener(HandleIncrementButtonClicked).Bind(ref _incrementButton)
                }
            }.AsFlexGroup().Use();
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            RefreshHandleAndTextColors();
        }

        private void RefreshHandleAndTextColors() {
            _handleImage.Color = Color.white.ColorWithAlpha(_interactable ? 0.8f : 0.3f);
            _text.Color = Color.white.ColorWithAlpha(_interactable ? 1f : 0.5f);
        }

        #endregion

        #region Callbacks

        private void HandleIncrementButtonClicked() {
            Value += ValueStep;
        }

        private void HandleDecrementButtonClicked() {
            Value -= ValueStep;
        }

        #endregion
    }
}