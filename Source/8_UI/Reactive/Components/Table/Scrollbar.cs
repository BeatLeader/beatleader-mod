using System;
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    /// <summary>
    /// Abstraction for list's scrollbar
    /// </summary>
    internal interface IScrollbar {
        float PageHeight { set; }
        float Progress { set; }

        bool CanScrollUp { set; }
        bool CanScrollDown { set; }

        event Action? ScrollBackwardButtonPressedEvent;
        event Action? ScrollForwardButtonPressedEvent;

        void SetActive(bool active);
    }

    /// <summary>
    /// Scrollbar for ReactiveComponent lists
    /// </summary>
    internal class Scrollbar : ReactiveComponent, IScrollbar {
        #region Impl

        float IScrollbar.PageHeight {
            set {
                _normalizedPageHeight = Mathf.Clamp01(value);
                RefreshHandle();
            }
        }

        float IScrollbar.Progress {
            set {
                _progress = Mathf.Clamp01(value);
                RefreshHandle();
            }
        }

        bool IScrollbar.CanScrollUp {
            set => _upButton.Interactable = value;
        }

        bool IScrollbar.CanScrollDown {
            set => _downButton.Interactable = value;
        }

        public event Action? ScrollBackwardButtonPressedEvent;
        public event Action? ScrollForwardButtonPressedEvent;

        void IScrollbar.SetActive(bool active) {
            Enabled = active;
        }

        #endregion

        #region Handle

        private float _padding = 0.25f;
        private float _progress;
        private float _normalizedPageHeight = 1f;

        private void RefreshHandle() {
            var num = _handleContainerRect.rect.size.y - 2f * _padding;
            _handleRect.sizeDelta = new Vector2(0f, _normalizedPageHeight * num);
            _handleRect.anchoredPosition = new Vector2(0f, -_progress * (1f - _normalizedPageHeight) * num - _padding);
        }

        #endregion

        #region Construct

        private RectTransform _handleContainerRect = null!;
        private RectTransform _handleRect = null!;
        private ButtonBase _upButton = null!;
        private ButtonBase _downButton = null!;

        protected override GameObject Construct() {
            static ButtonBase CreateButton(float rotation) {
                return new ImageButton {
                    Image = {
                        Sprite = BundleLoader.Sprites.transparentPixel,
                        Material = null
                    },
                    HoverLerpMul = float.MaxValue,
                    GrowOnHover = false,
                    Children = {
                        new Image {
                            ContentTransform = {
                                localEulerAngles = new(0f, 0f, rotation)
                            },
                            Sprite = GameResources.Sprites.ArrowIcon,
                            PreserveAspect = true,
                            Material = GameResources.UINoGlowMaterial
                        }.Export(out var image).AsFlexItem(size: 4f)
                    }
                }.AsFlexItem(grow: 1f).Export(out var button).WithListener(
                    x => x.Interactable,
                    _ => RefreshImage()
                ).WithAnimation(_ => RefreshImage());

                //rework after adding reactive animations
                void RefreshImage() {
                    var hovered = button.AnimationProgress > 0;
                    image.ContentTransform.localScale = hovered ? Vector3.one * 1.2f : Vector3.one;
                    image.Color = button.Interactable ?
                        hovered ? Color.white : Color.white.ColorWithAlpha(0.5f) : 
                        Color.black.ColorWithAlpha(0.5f);
                }
            }

            return new Dummy {
                Children = {
                    //handle container
                    new Image {
                        Sprite = BundleLoader.Sprites.background,
                        PixelsPerUnit = 20f,
                        Color = Color.black.ColorWithAlpha(0.5f),
                        Children = {
                            //handle
                            new Image {
                                ContentTransform = {
                                    anchorMin = new(0f, 1f),
                                    anchorMax = new(1f, 1f),
                                    pivot = new(0.5f, 1f)
                                },
                                Sprite = GameResources.Sprites.VerticalRoundRect,
                                Color = Color.white.ColorWithAlpha(0.5f),
                                ImageType = UnityEngine.UI.Image.Type.Sliced
                            }.Bind(ref _handleRect)
                        }
                    }.AsFlexItem(
                        grow: 1f,
                        margin: new() { left = "15%", right = "15%", top = 4f, bottom = 4f }
                    ).Bind(ref _handleContainerRect),
                    //
                    new Dummy {
                        Children = {
                            //up button
                            CreateButton(180f)
                                .WithClickListener(HandleUpButtonClicked)
                                .AsFlexGroup(alignItems: Align.FlexStart)
                                .Bind(ref _upButton),
                            //down button
                            CreateButton(0f)
                                .WithClickListener(HandleDownButtonClicked)
                                .AsFlexGroup(alignItems: Align.FlexEnd)
                                .Bind(ref _downButton)
                        }
                    }.AsFlexGroup(direction: FlexDirection.Column).WithRectExpand()
                }
            }.AsFlexGroup(FlexDirection.Column).Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem(size: new() { x = 2f });
            WithinLayoutIfDisabled = true;
            RefreshHandle();
        }

        #endregion

        #region Callbacks

        private void HandleUpButtonClicked() {
            ScrollBackwardButtonPressedEvent?.Invoke();
        }

        private void HandleDownButtonClicked() {
            ScrollForwardButtonPressedEvent?.Invoke();
        }

        #endregion
    }
}