using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ProfileBadgeOverview : ReactiveComponent {
        #region Props

        public sealed record Selection(PlayerBadge Badge, RectTransform Source);

        public Selection? SelectedBadge {
            get => _selection.Value;
            set => _selection.Value = value;
        }

        public float Progress {
            get => _progress.Value;
            set => _progress.Value = value;
        }

        public Action? OnClose { get; set; }

        #endregion

        #region Animation

        private RectTransform? _slot;

        private YogaFrame GetOffset(float progress, Selection? selection) {
            if (selection == null || _slot == null) {
                return new YogaFrame { left = 0f, top = 0f };
            }

            var source = selection.Source;
            var offset = (Vector2)_slot.InverseTransformPoint(source.TransformPoint(source.rect.center)) - _slot.rect.center;
            var factor = 1f - progress;

            return new YogaFrame { left = offset.x * factor, top = -offset.y * factor };
        }

        private Vector3 GetScale(float progress, Selection? selection) {
            if (selection == null || _slot == null || _slot.rect.width <= 0f) {
                return Vector3.one;
            }

            return Vector3.one * Mathf.LerpUnclamped(selection.Source.rect.width / _slot.rect.width, 1f, progress);
        }

        #endregion

        #region Construct

        private State<Selection?> _selection = null!;
        private State<float> _progress = null!;

        protected override GameObject Construct() {
            _selection = Remember<Selection?>(null);
            _progress = Remember(0f);

            var shown = _progress.Map(static x => x > 0.001f);
            var contentAlpha = _progress.Map(static x => Mathf.Clamp01(x * 2f - 1f));
            var badgeOffset = RememberDerived(_progress, _selection, GetOffset);
            var badgeScale = RememberDerived(_progress, _selection, GetScale);
            BsPrimaryTextButton openLinkButton = null!;

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column
                },

                FlexItem = {
                    PositionType = PositionType.Absolute,
                    Position = 0f
                },

                Children = {
                    new Background {
                        FlexItem = {
                            PositionType = PositionType.Absolute,
                            Position = 0f
                        },

                        Color = Color.clear,
                        sEnabled = shown
                    }.WithPointerEvents(onDown: _ => OnClose?.Invoke()),

                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column
                        },

                        FlexItem = {
                            PositionType = PositionType.Absolute,
                            Size = new() { x = 48f, y = 18f },
                            Position = new() { left = 50.pct, top = 30.pct },
                            Margin = new() { left = -24f, top = -9f }
                        },

                        Children = {
                            new WebImage {
                                FlexItem = {
                                    PositionType = PositionType.Absolute,
                                    Size = 100.pct,
                                    sPosition = badgeOffset
                                },

                                ContentTransform = {
                                    slocalScale = badgeScale
                                },

                                sEnabled = shown,
                                PreserveAspect = true,
                                sSrc = _selection.Map(static x => x?.Badge.image)
                            }
                        }
                    }.With(x => _slot = x.ContentTransform),

                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            AlignItems = Align.Center,
                            Gap = 2f
                        },

                        FlexItem = {
                            PositionType = PositionType.Absolute,
                            Position = new() { left = 0f, right = 0f, top = 30.pct },
                            Margin = new() { top = 11f }
                        },

                        sEnabled = shown,

                        Children = {
                            new Label {
                                FlexItem = {
                                    MaxSize = new() { x = 70f }
                                },

                                FontSize = 3.2f,
                                Alignment = TextAlignmentOptions.Center,
                                EnableWrapping = true,
                                sText = _selection.Map(static x => x?.Badge.description ?? ""),
                                sEnabled = _selection.Map(static x => x?.Badge.description is { Length: > 0 })
                            },

                            new Layout {
                                FlexController = {
                                    Gap = 1.5f
                                },

                                Children = {
                                    new BsPrimaryTextButton {
                                        FlexItem = {
                                            Size = new() { x = 20f, y = 6f }
                                        },

                                        Text = "Open link",
                                        sEnabled = _selection.Map(static x => x?.Badge.link is { Length: > 0 }),
                                        OnClick = () => {
                                            if (_selection.Value?.Badge.link is not { Length: > 0 } link) {
                                                return;
                                            }
                                            
                                            EnvironmentUtils.OpenBrowserPage(link);
                                            ActionBubble.Info(openLinkButton.ContentTransform, ProfileStyle.OpenedInBrowserText);
                                        }
                                    }.Bind(ref openLinkButton),

                                    new BsTextButton {
                                        FlexItem = {
                                            Size = new() { x = 20f, y = 6f }
                                        },

                                        Text = "Close",
                                        OnClick = () => OnClose?.Invoke()
                                    }
                                }
                            }
                        }
                    }.WithAlpha(contentAlpha)
                }
            }.Use();
        }

        #endregion
    }
}
