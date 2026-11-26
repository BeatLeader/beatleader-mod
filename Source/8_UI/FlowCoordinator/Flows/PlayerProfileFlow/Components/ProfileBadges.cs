using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using AnimationCurve = Reactive.AnimationCurve;
using SelectionMode = Reactive.Components.Basic.SelectionMode;

namespace BeatLeader.UI.Hub {
    internal class ProfileBadges : ReactiveComponent {
        #region Props

        public IReadOnlyList<PlayerBadge> Badges {
            get => _badges.Value;
            set => _badges.Value = value;
        }

        public PlayerBadge? HiddenBadge {
            get => _hiddenBadge.Value;
            set => _hiddenBadge.Value = value;
        }

        public float Time {
            get => _time.Value;
            set => _time.Value = value;
        }

        public Action<PlayerBadge, RectTransform>? OnBadgeSelected { get; set; }

        #endregion

        #region Construct

        private State<IReadOnlyList<PlayerBadge>> _badges = null!;
        private State<PlayerBadge?> _hiddenBadge = null!;
        private State<float> _time = null!;

        protected override GameObject Construct() {
            _badges = Remember<IReadOnlyList<PlayerBadge>>([]);
            _hiddenBadge = Remember<PlayerBadge?>(null);
            _time = Remember(0f);

            var stage = ProfileReveal.Stage(_time, 0.42f);

            return new Background {
                FlexController = {
                    Padding = 1f
                },

                FlexItem = {
                    sPosition = ProfileReveal.Slide(stage)
                },

                Children = {
                    new Repeater<PlayerBadge, WebImage> {
                        FlexController = {
                            FlexDirection = FlexDirection.Row,
                            FlexWrap = Wrap.Wrap,
                            JustifyContent = Justify.Center,
                            Gap = 1.5f
                        },

                        FlexItem = {
                            FlexGrow = 1f
                        },

                        SelectionMode = SelectionMode.None,
                        sItems = _badges,

                        ConstructCell = ctx => {
                            var appear = RememberDerived(ctx, _time, static (cell, x) => ProfileReveal.Progress(x, 0.45f + cell.Index * ProfileReveal.ItemDelay * 2f));
                            var alpha = RememberDerived(ctx, appear, _hiddenBadge, static (cell, x, hidden) => hidden == cell.Item ? 0f : x);
                            var hover = RememberAnimated(1f, 300.ms, AnimationCurve.EaseOutBack);
                            var scale = RememberDerived(appear, hover, static (x, y) => Vector3.one * Mathf.LerpUnclamped(0.3f, 1f, AnimationCurve.EaseOutBack.Evaluate(x)) * y);
                            var image = Ref<RectTransform>();

                            return new WebImage {
                                FlexItem = {
                                    Size = new() { x = 10.6f, y = 4f }
                                },

                                ContentTransform = {
                                    slocalScale = scale
                                },

                                sSrc = ctx.Map(static x => x.Item.image),
                                
                                PreserveAspect = true,
                                RaycastTarget = true,
                            }.Bind(image).WithAlpha(alpha).WithPointerEvents(
                                onEnter: _ => hover.TargetValue = 1.15f,
                                onLeave: _ => hover.TargetValue = 1f,
                                onDown: _ => {
                                    hover.SetValueImmediate(1f);
                                    global::Reactive.BeatSaber.GameResources.ButtonClickSignal.Raise();
                                    OnBadgeSelected?.Invoke(ctx.Item, image.Value!);
                                }
                            );
                        }
                    }
                }
            }.AsBlurBackground().WithAlpha(stage).Use();
        }

        #endregion
    }
}
