using System;
using BeatLeader.UI.Reactive.Yoga;
using HMUI;
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

        event Action<ScrollView.ScrollDirection>? ScrollEvent;

        void SetActive(bool active);
    }

    /// <summary>
    /// Scrollbar for ReactiveComponent lists
    /// </summary>
    internal class Scrollbar : ReactiveComponent, IScrollbar {
        #region Events

        public event Action<ScrollView.ScrollDirection>? ScrollEvent;

        #endregion

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
        private Button _upButton = null!;
        private Button _downButton = null!;

        protected override GameObject Construct() {
            static ImageButton CreateButton(float rotation) {
                return new ImageButton {
                    ContentTransform = {
                        localEulerAngles = new(0f, 0f, rotation),
                        localScale = Vector3.one * 1.2f
                    },
                    Image = {
                        Sprite = GameResources.Sprites.ArrowIcon,
                        PreserveAspect = true,
                        Material = GameResources.UINoGlowMaterial
                    },
                    HoverLerpMul = float.MaxValue,
                    HoverScaleSum = Vector3.one * 0.2f,
                    Colors = new() {
                        Color = Color.white.ColorWithAlpha(0.5f),
                        HoveredColor = Color.white,
                        DisabledColor = Color.black.ColorWithAlpha(0.5f)
                    }
                }.AsFlexItem(basis: 4f);
            }
            
            return new Dummy {
                Children = {
                    //up button
                    CreateButton(180f)
                        .WithClickListener(HandleUpButtonClicked)
                        .Bind(ref _upButton),
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
                        margin: new() { left = "15%", right = "15%" }
                    ).Bind(ref _handleContainerRect),
                    //down button
                    CreateButton(0f)
                        .WithClickListener(HandleDownButtonClicked)
                        .Bind(ref _downButton)
                }
            }.AsFlexGroup(FlexDirection.Column).Use();
        }

        protected override void OnInitialize() {
            WithinLayoutIfDisabled = true;
            RefreshHandle();
        }

        #endregion

        #region Callbacks

        private void HandleUpButtonClicked() {
            ScrollEvent?.Invoke(ScrollView.ScrollDirection.Up);
        }

        private void HandleDownButtonClicked() {
            ScrollEvent?.Invoke(ScrollView.ScrollDirection.Down);
        }

        #endregion
    }
}