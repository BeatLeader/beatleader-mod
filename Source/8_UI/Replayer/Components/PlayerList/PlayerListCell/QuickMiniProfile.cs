using System;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using RDummy = BeatLeader.UI.Reactive.Components.Dummy;

namespace BeatLeader.Components {
    internal class QuickMiniProfile : ReeUIComponentV3<QuickMiniProfile> {
        #region Layout

        public Justify JustifyContent {
            get => _layoutController.JustifyContent;
            set {
                switch (value) {
                    case Justify.FlexStart:
                        _railsLayoutController.UseIndependentLayout = true;
                        _railsLayoutModifier.FlexGrow = 1;
                        _railsLayoutModifier.Size = new() { x = YogaValue.Undefined };
                        break;
                    case Justify.Center:
                        _railsLayoutController.UseIndependentLayout = false;
                        _railsLayoutModifier.FlexGrow = 0;
                        _railsLayoutModifier.Size = new() { x = "auto" };
                        break;
                    default:
                        throw new Exception("Only FlexStart and Center are supported");
                }
                _layoutController.JustifyContent = value;
            }
        }

        #endregion

        #region Setup

        public IPlayer? Player { get; private set; }

        public void SetPlayer(IPlayer? player) {
            Player = player;
            _playerAvatar.SetAvatar(player);
            _playerCountryFlag.SetCountry(player?.Country ?? "not set");

            _playerNameLabel.Text = player?.Name ?? "Loading...";
            _playerGlobalRankLabel.Text = FormatUtils.FormatRank(player?.Rank ?? -1, true);
            _playerPpLabel.Text = FormatUtils.FormatPP(player?.PerformancePoints ?? -1);
        }

        #endregion

        #region Construct

        private PlayerAvatar _playerAvatar = null!;
        private CountryFlag _playerCountryFlag = null!;
        private Label _playerNameLabel = null!;
        private Label _playerGlobalRankLabel = null!;
        private Label _playerPpLabel = null!;
        private YogaLayoutController _layoutController = null!;
        private YogaLayoutController _railsLayoutController = null!;
        private YogaModifier _railsLayoutModifier = null!;

        protected override GameObject Construct(Transform parent) {
            static ReactiveComponentBase CreateFitter(
                params ILayoutItem[] children
            ) {
                var dummy = new RDummy()
                    .WithBlurBackground()
                    .AsFlexGroup(
                        padding: new() { left = 1f, right = 1f },
                        gap: new() { x = 1f },
                        overflow: Overflow.Hidden
                    ).AsFlexItem(
                        size: "auto",
                        maxSize: "100%"
                    );
                dummy.Children.AddRange(children);
                return dummy;
            }

            static ReactiveComponentBase CreateRail(
                params ILayoutItem[] children
            ) {
                var dummy = new RDummy()
                    .AsFlexGroup(
                        justifyContent: Justify.FlexStart,
                        gap: new() { x = 0.5f },
                        expandUnspecifiedChildren: false
                    );
                dummy.Children.AddRange(children);
                return dummy;
            }

            return new RDummy {
                Children = {
                    //avatar
                    new ReeWrapperV2<PlayerAvatar>()
                        .AsFlexItem(aspectRatio: 1f)
                        .BindRee(ref _playerAvatar),
                    //infos
                    new RDummy {
                        Children = {
                            //player & country
                            CreateRail(
                                CreateFitter(
                                    //flag
                                    new ReeWrapperV2<CountryFlag>()
                                        .AsFlexItem(minSize: new() { x = 4f })
                                        .BindRee(ref _playerCountryFlag),
                                    //name text
                                    new Label {
                                        Overflow = TextOverflowModes.Ellipsis,
                                        FontSize = 3
                                    }.AsFlexItem(size: "auto").Bind(ref _playerNameLabel)
                                )
                            ).AsFlexItem(size: new() { y = "auto" }),
                            //rank & pp
                            CreateRail(
                                //rank container
                                CreateFitter(
                                    //rank text
                                    new Label {
                                        FontSize = 3
                                    }.AsFlexItem(size: "auto").Bind(ref _playerGlobalRankLabel)
                                ),
                                //pp container
                                CreateFitter(
                                    //pp text
                                    new Label {
                                        FontSize = 3
                                    }.AsFlexItem(size: "auto").Bind(ref _playerPpLabel)
                                )
                            ).AsFlexItem(size: new() { y = "auto" })
                        }
                    }.AsFlexGroup(
                        direction: UI.Reactive.Yoga.FlexDirection.Column,
                        justifyContent: Justify.Center,
                        alignItems: Align.FlexStart,
                        gap: new() { y = 1f },
                        expandUnspecifiedChildren: false,
                        layoutController: out _railsLayoutController
                    ).AsFlexItem(
                        modifier: out _railsLayoutModifier
                    )
                }
            }.AsFlexGroup(
                gap: new() { x = 2f },
                padding: 1f,
                layoutController: out _layoutController
            ).With(
                x => {
                    x.Content.AddComponent<LayoutElement>().preferredHeight = 18;
                }
            ).Use(parent);
        }

        protected override void OnInitialize() {
            JustifyContent = Justify.FlexStart;
        }

        #endregion
    }
}