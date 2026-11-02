using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Components.Basic;
using Reactive.Yoga;
using TMPro;
using UnityEngine;
using Background = Reactive.BeatSaber.Components.Background;
using Image = Reactive.BeatSaber.Components.Image;
using Label = Reactive.BeatSaber.Components.Label;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleOpponentsList : ReactiveComponent {
        #region Cells

        private readonly record struct Opponent(BattleRoyaleReplay Replay, int Rank);

        private class Cell : ReactiveComponent {
            public Cell(CellContext<Opponent> context, BattleRoyaleOpponentsList list) : base(false) {
                _list = list;
                ConstructAndInit();
                context.AddCallback(this, static (x, y) => x.HandleItemUpdated(y.Item));
            }

            #region Construct

            private PlayerAvatar _playerAvatar = null!;
            private Label _playerNameText = null!;
            private Label _rankText = null!;
            private Label _dateText = null!;

            protected override GameObject Construct() {
                _progress = RememberAnimated(0f, 10.fact);
                _accentColor = Remember<Color?>(null);
                _progress.AddCallback(this, static (x, y) => x.Pulse(y), lazy: true);

                return new Layout {
                    Children = {
                        new Background {
                            Sprite = BundleLoader.Sprites.background,
                            sColor = RememberDerived(
                                _progress, _accentColor,
                                static (progress, accentColor) => Color.Lerp(
                                    Color.white.ColorWithAlpha(0.1f),
                                    accentColor ?? Color.white.ColorWithAlpha(0.3f),
                                    progress
                                )
                            ),
                            PixelsPerUnit = 8f,
                            Skew = BeatSaberStyle.Skew,

                            Children = {
                                //rank
                                new Label {
                                    LayoutModifier = new YogaModifier {
                                        Margin = new() { left = 1.pt, right = 1.pt },
                                        AlignSelf = Align.Center
                                    },

                                    FontSize = 5f
                                }.Bind(ref _rankText),

                                //avatar
                                new ReeWrapperV2<PlayerAvatar>()
                                    .AsFlexItem(aspectRatio: 1f)
                                    .BindRee(ref _playerAvatar),

                                //texts
                                new Layout {
                                    Children = {
                                        //player name
                                        new Label {
                                                FontSize = 5f,
                                                Overflow = TextOverflowModes.Ellipsis,
                                                Alignment = TextAlignmentOptions.Left,
                                                FontStyle = FontStyles.Italic
                                            }
                                            .AsFlexItem(flexGrow: 1f)
                                            .Bind(ref _playerNameText),

                                        //replay date
                                        new Label {
                                                Color = UIStyle.SecondaryTextColor,
                                                FontSize = 4f,
                                                Overflow = TextOverflowModes.Ellipsis,
                                                Alignment = TextAlignmentOptions.Right,
                                                FontStyle = FontStyles.Italic
                                            }
                                            .AsFlexItem(flexGrow: 1f)
                                            .Bind(ref _dateText),
                                    }
                                }.AsFlexGroup().AsFlexItem(
                                    flexGrow: 1f,
                                    margin: new() { left = 2f, right = 2f }
                                ),

                                //remove button
                                new BsImageButton {
                                    LayoutModifier = new YogaModifier {
                                        Size = new() { x = 6.pt }
                                    },

                                    Sprite = BundleLoader.Sprites.crossIcon,
                                    ShowUnderline = false,
                                    OnClick = HandleRemoveButtonClicked,
                                    sEnabled = _list._canMutateLobby
                                },

                                //navigate button
                                new BsImageButton {
                                    LayoutModifier = new YogaModifier {
                                        Size = new() { x = 6.pt }
                                    },

                                    Sprite = BundleLoader.Sprites.rightArrowIcon,
                                    ShowUnderline = false,
                                    OnClick = HandleNavigateButtonClicked,
                                    sEnabled = _list._canMutateLobby
                                }
                            }
                        }.AsFlexGroup(
                            padding: 1f,
                            gap: 1f
                        ).AsFlexItem(
                            flexGrow: 1f,
                            margin: new() { right = 1f, left = 1f }
                        )
                    }
                }.AsFlexGroup().AsFlexItem(size: new() { y = 10f }).Use();
            }

            #endregion

            #region Animation

            private AnimatedState<float> _progress = null!;
            private State<Color?> _accentColor = null!;

            private void Pulse(float progress) {
                if (_accentColor.Value != null) {
                    return;
                }

                if (progress > 0.8f) {
                    _progress.TargetValue = 0f;
                } else if (progress < 0.2f) {
                    _progress.TargetValue = 1f;
                }
            }

            private void ResetColor() {
                _accentColor.Value = null;
                _progress.TargetValue = _progress.Value > 0.5f ? 0f : 1f;
            }

            private void SetColor(Color color) {
                _accentColor.Value = color.ColorWithAlpha(0.2f);
                _progress.TargetValue = 1f;
            }

            #endregion

            #region Setup

            private readonly BattleRoyaleOpponentsList _list;
            private Opponent _item;
            private CancellationTokenSource _tokenSource = new();
            private Task? _refreshColorTask;
            private Task? _refreshPlayerTask;

            private void HandleItemUpdated(Opponent item) {
                if (item == _item) {
                    return;
                }

                if (_refreshColorTask != null || _refreshPlayerTask != null) {
                    _tokenSource.Cancel();
                    _tokenSource = new();
                }

                _item = item;

                ResetColor();
                RefreshOtherText();

                var token = _tokenSource.Token;

                _refreshPlayerTask = RefreshPlayer(item.Replay, token).RunCatching();
                _refreshColorTask = RefreshAccentColor(item.Replay, token).RunCatching();
            }

            private async Task RefreshAccentColor(BattleRoyaleReplay replay, CancellationToken token) {
                var data = await replay.GetBattleRoyaleDataAsync(false, token);

                if (token.IsCancellationRequested) {
                    return;
                }

                var color = data.AccentColor ?? Color.white;
                SetColor(color);
                _refreshColorTask = null;
            }

            private async Task RefreshPlayer(BattleRoyaleReplay replay, CancellationToken token) {
                var player = await replay.ReplayHeader.LoadPlayerAsync(false, token) as IPlayer;

                if (token.IsCancellationRequested) {
                    return;
                }

                _playerAvatar.SetAvatar(player);
                _playerNameText.Text = player.Name;
                _refreshPlayerTask = null;
            }

            private void RefreshOtherText() {
                _rankText.Text = $"#{_item.Rank}";
                var timestamp = _item.Replay.ReplayHeader.ReplayInfo.Timestamp.ToString();
                _dateText.Text = FormatUtils.FormatTimeset(timestamp, false);
            }

            #endregion

            #region Callbacks

            private void HandleNavigateButtonClicked() {
                _list._battleRoyaleHost?.NavigateTo(_item.Replay.ReplayHeader);
            }

            private void HandleRemoveButtonClicked() {
                _list._battleRoyaleHost?.RemoveReplay(_item.Replay.ReplayHeader, this);
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
                _battleRoyaleHost.CanMutateLobbyStateChangedEvent -= HandleCanMutateLobbyChangedEvent;
            }

            _battleRoyaleHost = battleRoyaleHost;

            if (_battleRoyaleHost != null) {
                _battleRoyaleHost.ReplayAddedEvent += HandleReplayAdded;
                _battleRoyaleHost.ReplayRemovedEvent += HandleReplayRemoved;
                _battleRoyaleHost.ReplayRefreshRequestedEvent += HandleRefreshRequested;
                _battleRoyaleHost.CanMutateLobbyStateChangedEvent += HandleCanMutateLobbyChangedEvent;

                HandleCanMutateLobbyChangedEvent(_battleRoyaleHost.CanMutateLobby);
            }
        }

        #endregion

        #region Construct

        private readonly List<BattleRoyaleReplay> _replays = new();
        private State<bool> _canMutateLobby = null!;
        private Table<Opponent, Cell> _table = null!;

        protected override GameObject Construct() {
            _canMutateLobby = Remember(true);
            var scrollContext = new ScrollContext();

            return new Layout {
                Children = {
                    new Table<Opponent, Cell> {
                        ScrollContext = scrollContext,
                        SelectionMode = SelectionMode.None,
                        Items = Array.Empty<Opponent>(),
                        ConstructCell = ctx => new Cell(ctx, this)
                    }.AsFlexItem(flexGrow: 1f).Bind(ref _table),

                    new Scrollbar {
                        ScrollContext = scrollContext,
                        HideIfNothingToScroll = true,
                        WithinLayoutIfDisabled = false
                    }.AsFlexItem()
                }
            }.AsFlexGroup(gap: 1f).Use();
        }

        private void RefreshItems() {
            _table.Items = _replays.Select(static x => new Opponent(x, x.ReplayRank)).ToArray();
        }

        #endregion

        #region Comparison

        private class OpponentComparator : IComparer<BattleRoyaleReplay> {
            public int Compare(BattleRoyaleReplay x, BattleRoyaleReplay y) {
                return x.ReplayRank.CompareTo(y.ReplayRank);
            }
        }

        private static readonly OpponentComparator opponentComparator = new();

        #endregion

        #region Callbacks

        private void HandleReplayAdded(BattleRoyaleReplay replay, object caller) {
            _replays.Add(replay);
            RefreshItems();
        }

        private void HandleReplayRemoved(BattleRoyaleReplay replay, object caller) {
            _replays.Remove(replay);
            RefreshItems();
        }

        private void HandleRefreshRequested() {
            _replays.Sort(opponentComparator);
            RefreshItems();
        }

        private void HandleCanMutateLobbyChangedEvent(bool canMutate) {
            _canMutateLobby.Value = canMutate;
        }

        #endregion
    }
}