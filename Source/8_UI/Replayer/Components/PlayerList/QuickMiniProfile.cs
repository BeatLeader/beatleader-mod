using System;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Compiler;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI {
    internal class QuickMiniProfile : ReactiveComponent {
        #region Public API

        /// <summary>
        /// Only <see cref="Justify.FlexStart"/> and <see cref="Justify.Center"/> are supported.
        /// </summary>
        public Justify JustifyContent {
            get => _justifyContent.Value;
            set {
                if (value is not (Justify.FlexStart or Justify.Center)) {
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Only FlexStart and Center are supported");
                }
                _justifyContent.Value = value;
            }
        }

        public bool UseAlternativeBlur {
            get => _alternativeBlur.Value;
            set => _alternativeBlur.Value = value;
        }

        public IPlayer? Player {
            get => _player.Value;
            set => _player.Value = value;
        }

        /// <summary>
        /// Shows placeholders instead of the player while it loads.
        /// </summary>
        public bool IsLoading {
            get => _loading.Value;
            set => _loading.Value = value;
        }

        #endregion

        #region Construct

        private State<Justify> _justifyContent = null!;
        private State<bool> _alternativeBlur = null!;
        private State<IPlayer?> _player = null!;
        private State<bool> _loading = null!;

        protected override GameObject Construct() {
            static ReactiveComponent CreateRail(params ILayoutItem[] children) {
                var layout = new Layout {
                    FlexController = {
                        JustifyContent = Justify.FlexStart,
                        Gap = new() { x = 0.5f }
                    }
                };
                layout.Children.AddRange(children);
                return layout;
            }

            _justifyContent = Remember(Justify.FlexStart);
            _alternativeBlur = Remember(false);
            _player = Remember<IPlayer?>(null);
            _loading = Remember(false);

            var profile = RememberDerived(_player, _loading, static (player, loading) => (player, loading));

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Row,
                    Gap = new() { x = 2f },
                    Padding = 1f,
                    sJustifyContent = _justifyContent
                },

                Children = {
                    //avatar
                    new ReeWrapperV2<PlayerAvatar> {
                        FlexItem = {
                            AspectRatio = 1f
                        }
                    }.On(profile, static (x, y) => {
                        if (y.loading) {
                            x.ReeComponent.SetLoading();
                        } else {
                            x.ReeComponent.SetAvatar(y.player);
                        }
                    }),
                    //infos
                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            JustifyContent = Justify.Center,
                            AlignItems = Align.FlexStart,
                            Gap = new() { y = 1f }
                        },

                        FlexItem = {
                            // Stretches the rails when aligned to the start, otherwise fits them to be centered
                            sFlexGrow = _justifyContent.Map(static x => x == Justify.FlexStart ? 1f : 0f),
                            sSize = _justifyContent.Map(
                                static x => new YogaVector { x = x == Justify.FlexStart ? YogaValue.Undefined : YogaValue.Auto }
                            )
                        },

                        Children = {
                            //player & country
                            CreateRail(
                                CreateFitter(
                                    _alternativeBlur,
                                    //flag
                                    new ReeWrapperV2<CountryFlag> {
                                        FlexItem = {
                                            MinSize = new() { x = 4f }
                                        },

                                        sEnabled = _loading.Map(static x => !x)
                                    }.On(_player, static (x, y) => x.ReeComponent.SetCountry(y?.Country ?? "not set")),
                                    //name text
                                    new Label {
                                        FlexItem = {
                                            Size = YogaVector.Auto
                                        },

                                        Overflow = TextOverflowModes.Ellipsis,
                                        FontSize = 3,
                                        sText = profile.Map(static x => x.loading ? "Loading..." : x.player?.Name ?? "Unknown")
                                    }
                                )
                            ).AsFlexItem(size: new() { y = YogaValue.Auto }),
                            //rank & pp
                            CreateRail(
                                //rank container
                                CreateFitter(
                                    _alternativeBlur,
                                    //rank text
                                    new Label {
                                        FlexItem = {
                                            Size = YogaVector.Auto
                                        },

                                        FontSize = 3,
                                        sText = profile.Map(static x => FormatUtils.FormatRank(x.loading ? -1 : x.player?.Rank ?? -1, true))
                                    }
                                ),
                                //pp container
                                CreateFitter(
                                    _alternativeBlur,
                                    //pp text
                                    new Label {
                                        FlexItem = {
                                            Size = YogaVector.Auto
                                        },

                                        FontSize = 3,
                                        sText = profile.Map(static x => FormatUtils.FormatPP(x.loading ? -1 : x.player?.PerformancePoints ?? -1))
                                    }
                                )
                            ).AsFlexItem(size: new() { y = YogaValue.Auto })
                        }
                    }
                }
            }.Use();
        }

        [StateGen]
        private static ReactiveComponent CreateFitter(IState<bool> alternativeBlur, params ILayoutItem[] children) {
            var layout = new Background {
                FlexController = {
                    Padding = new() { left = 1f, right = 1f },
                    Gap = new() { x = 1f }
                },

                FlexItem = {
                    Size = YogaVector.Auto,
                    MaxSize = 100.pct
                },

                sMaterial = alternativeBlur.Map(
                    static x => x ? BundleLoader.Materials.tintedBlurredBackgroundMaterial : GameResources.UIFogBackgroundMaterial
                )
            }.AsBlurBackground(pixelsPerUnit: 12f);

            layout.Children.AddRange(children);
            return layout;
        }

        #endregion
    }
}
