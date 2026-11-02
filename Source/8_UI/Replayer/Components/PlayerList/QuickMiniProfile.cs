using System;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI {
    internal class QuickMiniProfile : ReactiveComponent {
        #region Layout

        public Justify JustifyContent {
            get => _root.FlexController.JustifyContent;
            set {
                switch (value) {
                    case Justify.FlexStart:
                        _rails.FlexItem.FlexGrow = 1;
                        _rails.FlexItem.Size = new() { x = YogaValue.Undefined };
                        break;
                    case Justify.Center:
                        _rails.FlexItem.FlexGrow = 0;
                        _rails.FlexItem.Size = new() { x = "auto" };
                        break;
                    default:
                        throw new Exception("Only FlexStart and Center are supported");
                }
                _root.FlexController.JustifyContent = value;
            }
        }

        public bool UseAlternativeBlur {
            set {
                var mat = value ? 
                    BundleLoader.Materials.tintedBlurredBackgroundMaterial :
                    GameResources.UIFogBackgroundMaterial;
                _nameFitterBackground.Material = mat;
                _rankFitterBackground.Material = mat;
                _ppFitterBackground.Material = mat;
            }
        }

        #endregion

        #region Setup

        public IPlayer? Player { get; private set; }

        public void SetPlayer(IPlayer? player) {
            Player = player;
            _playerAvatar.SetAvatar(player);
            _playerCountryFlag.SetCountry(player?.Country ?? "not set");
            _playerCountryFlag.GetRootTransform().gameObject.SetActive(true);

            _playerNameLabel.Text = player?.Name ?? "Unknown";
            _playerGlobalRankLabel.Text = FormatUtils.FormatRank(player?.Rank ?? -1, true);
            _playerPpLabel.Text = FormatUtils.FormatPP(player?.PerformancePoints ?? -1);
        }

        public void SetLoading() {
            _playerAvatar.SetLoading();
            _playerCountryFlag.GetRootTransform().gameObject.SetActive(false);

            _playerNameLabel.Text = "Loading...";
            _playerGlobalRankLabel.Text = FormatUtils.FormatRank(-1, true);
            _playerPpLabel.Text = FormatUtils.FormatPP(-1);
        }

        #endregion

        #region Construct

        private PlayerAvatar _playerAvatar = null!;
        private CountryFlag _playerCountryFlag = null!;
        private Label _playerNameLabel = null!;
        private Label _playerGlobalRankLabel = null!;
        private Label _playerPpLabel = null!;
        private Layout _root = null!;
        private Layout _rails = null!;
        private Image _nameFitterBackground = null!;
        private Image _rankFitterBackground = null!;
        private Image _ppFitterBackground = null!;

        protected override GameObject Construct() {
            static ReactiveComponent CreateFitter(
                out Image background,
                params ILayoutItem[] children
            ) {
                var layout = new Background {
                    FlexController = {
                        Padding = new() { left = 1f, right = 1f },
                        Gap = new() { x = 1f }
                    },

                    FlexItem = {
                        Size = "auto",
                        MaxSize = "100%"
                    }
                }.Export(out background).AsBlurBackground(pixelsPerUnit: 12f);
                layout.Children.AddRange(children);
                return layout;
            }

            static ReactiveComponent CreateRail(
                params ILayoutItem[] children
            ) {
                var layout = new Layout {
                    FlexController = {
                        JustifyContent = Justify.FlexStart,
                        Gap = new() { x = 0.5f }
                    }
                };
                layout.Children.AddRange(children);
                return layout;
            }

            return new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Row,
                    Gap = new() { x = 2f },
                    Padding = 1f
                },

                Children = {
                    //avatar
                    new ReeWrapperV2<PlayerAvatar> {
                        FlexItem = {
                            AspectRatio = 1f
                        }
                    }.BindRee(ref _playerAvatar),
                    //infos
                    new Layout {
                        FlexController = {
                            FlexDirection = FlexDirection.Column,
                            JustifyContent = Justify.Center,
                            AlignItems = Align.FlexStart,
                            Gap = new() { y = 1f }
                        },

                        Children = {
                            //player & country
                            CreateRail(
                                CreateFitter(
                                    out _nameFitterBackground,
                                    //flag
                                    new ReeWrapperV2<CountryFlag> {
                                        FlexItem = {
                                            MinSize = new() { x = 4f }
                                        }
                                    }.BindRee(ref _playerCountryFlag),
                                    //name text
                                    new Label {
                                        FlexItem = {
                                            Size = "auto"
                                        },

                                        Overflow = TextOverflowModes.Ellipsis,
                                        FontSize = 3
                                    }.Bind(ref _playerNameLabel)
                                )
                            ).AsFlexItem(size: new() { y = "auto" }),
                            //rank & pp
                            CreateRail(
                                //rank container
                                CreateFitter(
                                    out _rankFitterBackground,
                                    //rank text
                                    new Label {
                                        FlexItem = {
                                            Size = "auto"
                                        },

                                        FontSize = 3
                                    }.Bind(ref _playerGlobalRankLabel)
                                ),
                                //pp container
                                CreateFitter(
                                    out _ppFitterBackground,
                                    //pp text
                                    new Label {
                                        FlexItem = {
                                            Size = "auto"
                                        },

                                        FontSize = 3
                                    }.Bind(ref _playerPpLabel)
                                )
                            ).AsFlexItem(size: new() { y = "auto" })
                        }
                    }.Bind(ref _rails)
                }
            }.Bind(ref _root).Use();
        }

        protected override void OnInitialize() {
            JustifyContent = Justify.FlexStart;
            UseAlternativeBlur = false;
        }

        #endregion
    }
}