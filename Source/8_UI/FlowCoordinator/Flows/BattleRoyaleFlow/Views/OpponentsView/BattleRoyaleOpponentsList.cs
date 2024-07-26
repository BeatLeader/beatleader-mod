using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.Components;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using TMPro;
using UnityEngine;
using Dummy = BeatLeader.UI.Reactive.Components.Dummy;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleOpponentsList : Table<IBattleRoyaleReplay, BattleRoyaleOpponentsList.Cell> {
        #region Cells

        public class Cell : TableComponentCell<IBattleRoyaleReplay> {
            #region Construct

            private Image _backgroundImage = null!;
            private PlayerAvatar _playerAvatar = null!;
            private Label _playerNameText = null!;
            private Label _rankText = null!;
            private Label _dateText = null!;

            protected override GameObject Construct() {
                return new Dummy {
                    Children = {
                        new Image {
                            Sprite = BundleLoader.Sprites.background,
                            Color = Color.white.ColorWithAlpha(0.2f),
                            PixelsPerUnit = 8f,
                            Skew = UIStyle.Skew,
                            Children = {
                                //rank
                                new Label {
                                    FontSize = 5f
                                }.AsFlexItem(
                                    size: "auto",
                                    margin: new() { left = 1f, right = 1f },
                                    alignSelf: Align.Center
                                ).Bind(ref _rankText),
                                //avatar
                                new ReeWrapperV2<PlayerAvatar>()
                                    .AsFlexItem(aspectRatio: 1f)
                                    .BindRee(ref _playerAvatar),
                                //texts
                                new Dummy {
                                    Children = {
                                        //player name
                                        new Label {
                                                FontSize = 5f,
                                                Overflow = TextOverflowModes.Ellipsis,
                                                Alignment = TextAlignmentOptions.Left,
                                                FontStyle = FontStyles.Italic
                                            }
                                            .AsFlexItem(grow: 1f)
                                            .Bind(ref _playerNameText),
                                        //replay date
                                        new Label {
                                                Color = UIStyle.SecondaryTextColor,
                                                FontSize = 4f,
                                                Overflow = TextOverflowModes.Ellipsis,
                                                Alignment = TextAlignmentOptions.Right,
                                                FontStyle = FontStyles.Italic
                                            }
                                            .AsFlexItem(grow: 1f)
                                            .Bind(ref _dateText),
                                    }
                                }.AsFlexGroup().AsFlexItem(
                                    grow: 1f,
                                    margin: new() { left = 2f, right = 2f }
                                ),
                                //remove button
                                new BsButton {
                                        ShowUnderline = false,
                                        Skew = UIStyle.Skew
                                    }
                                    .AsFlexGroup(padding: 1f)
                                    .WithImage(
                                        sprite: BundleLoader.Sprites.crossIcon,
                                        color: UIStyle.SecondaryTextColor
                                    )
                                    .WithClickListener(HandleRemoveButtonClicked)
                                    .AsFlexItem(basis: 6f),
                                //navigate button
                                new BsButton {
                                        ShowUnderline = false,
                                        Skew = UIStyle.Skew
                                    }
                                    .AsFlexGroup(padding: 0.5f)
                                    .WithImage(
                                        sprite: BundleLoader.Sprites.rightArrowIcon,
                                        color: UIStyle.SecondaryTextColor
                                    )
                                    .WithClickListener(HandleNavigateButtonClicked)
                                    .AsFlexItem(basis: 6f)
                            }
                        }.AsFlexGroup(
                            padding: 1f,
                            gap: 1f
                        ).AsFlexItem(
                            grow: 1f,
                            margin: new() { right = 1f, left = 1f }
                        ).Bind(ref _backgroundImage)
                    }
                }.AsFlexGroup().WithSizeDelta(0f, 10f).Use();
            }

            #endregion

            #region Setup

            private IBattleRoyaleHost _battleRoyaleHost = null!;

            protected override void OnInit(IBattleRoyaleReplay item) {
                RefreshPlayer();
                RefreshAccentColor();
            }

            public void Init(IBattleRoyaleHost battleRoyaleHost) {
                _battleRoyaleHost = battleRoyaleHost;
            }

            private async void RefreshAccentColor() {
                var data = await Item.GetReplayDataAsync();
                var color = data.AccentColor ?? Color.white;
                _backgroundImage.Color = color.ColorWithAlpha(0.2f);
            }
            
            private async void RefreshPlayer() {
                var header = Item.ReplayHeader;
                var player = await header.LoadPlayerAsync(false, default);
                _playerAvatar.SetAvatar(player);
                _playerNameText.Text = player.Name;
                _rankText.Text = $"#{Item.ReplayRank}";
                var timestamp = header.ReplayInfo.Timestamp.ToString();
                _dateText.Text = FormatUtils.FormatTimeset(timestamp, false);
            }

            #endregion

            #region Callbacks

            private void HandleNavigateButtonClicked() {
                _battleRoyaleHost.NavigateTo(Item.ReplayHeader);
            }

            private void HandleRemoveButtonClicked() {
                _battleRoyaleHost.RemoveReplay(Item.ReplayHeader, this);
            }

            #endregion
        }

        #endregion

        #region Setup

        private IBattleRoyaleHost? _battleRoyaleHost;

        public void Setup(IBattleRoyaleHost? battleRoyaleHost) {
            if (_battleRoyaleHost != null) {
                _battleRoyaleHost.ReplayAddedEvent -= HandleReplayAdded;
                _battleRoyaleHost.ReplayRemovedEvent -= HandleReplayRemoved;
                _battleRoyaleHost.ReplayRefreshRequestedEvent -= HandleRefreshRequested;
            }
            _battleRoyaleHost = battleRoyaleHost;
            if (_battleRoyaleHost != null) {
                _battleRoyaleHost.ReplayAddedEvent += HandleReplayAdded;
                _battleRoyaleHost.ReplayRemovedEvent += HandleReplayRemoved;
                _battleRoyaleHost.ReplayRefreshRequestedEvent += HandleRefreshRequested;
            }
        }

        protected override void OnCellConstruct(Cell cell) {
            ValidateAndThrow();
            cell.Init(_battleRoyaleHost!);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            SelectionMode = SelectionMode.None;
        }

        protected override bool Validate() {
            return _battleRoyaleHost is not null;
        }

        #endregion

        #region Comparison

        private class OpponentComparator : IComparer<IBattleRoyaleReplay> {
            public int Compare(IBattleRoyaleReplay x, IBattleRoyaleReplay y) {
                return x.ReplayRank.CompareTo(y.ReplayRank);
            }
        }

        private static readonly OpponentComparator opponentComparator = new();

        #endregion

        #region Callbacks

        private void HandleReplayAdded(IBattleRoyaleReplay replay, object caller) {
            Items.Add(replay);
            Refresh();
        }

        private void HandleReplayRemoved(IBattleRoyaleReplay replay, object caller) {
            Items.Remove(replay);
            Refresh();
        }

        private void HandleRefreshRequested() {
            Items.Sort(opponentComparator);
            Refresh();
        }

        #endregion
    }
}