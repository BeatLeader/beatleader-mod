
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleOpponentsList : Table<BattleRoyaleReplay, BattleRoyaleOpponentsList.Cell> {
        #region Cells

        public class Cell : TableComponentCell<BattleRoyaleReplay> {
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
                                        Skew = UIStyle.Skew,
                                        OnClick = HandleRemoveButtonClicked
                                    }
                                    .AsFlexGroup(padding: 1f)
                                    .WithImage(
                                        sprite: BundleLoader.Sprites.crossIcon,
                                        color: UIStyle.SecondaryTextColor
                                    )
                                    .AsFlexItem(basis: 6f),
                                //navigate button
                                new BsButton {
                                        ShowUnderline = false,
                                        Skew = UIStyle.Skew,
                                        OnClick = HandleNavigateButtonClicked
                                    }
                                    .AsFlexGroup(padding: 0.5f)
                                    .WithImage(
                                        sprite: BundleLoader.Sprites.rightArrowIcon,
                                        color: UIStyle.SecondaryTextColor
                                    )
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

            #region Animation

            private readonly ValueAnimator _valueAnimator = new();
            private Color _targetColor;
            private bool _colorShouldBeSet;
            private bool _colorSet;

            protected override void OnUpdate() {
                if (_colorSet) return;
                //
                _valueAnimator.Update();
                if (!_colorShouldBeSet) {
                    if (_valueAnimator.Progress > 0.8f) {
                        _valueAnimator.SetTarget(0f);
                    } else if (_valueAnimator.Progress < 0.2f) {
                        _valueAnimator.SetTarget(1f);
                    }
                }
                //
                var targetColor = _colorShouldBeSet ? _targetColor : Color.white.ColorWithAlpha(0.3f);
                var color = Color.Lerp(
                    Color.white.ColorWithAlpha(0.1f),
                    targetColor,
                    _valueAnimator.Progress
                );
                _backgroundImage.Color = color;
                if (_colorShouldBeSet && color == _targetColor) {
                    _colorSet = true;
                }
            }

            private void ResetColor() {
                _colorSet = false;
                _colorShouldBeSet = false;
                OnUpdate();
            }

            private void SetColor(Color color) {
                _targetColor = color.ColorWithAlpha(0.2f);
                _colorShouldBeSet = true;
                _valueAnimator.SetTarget(1f);
            }

            #endregion

            #region Setup

            private IBattleRoyaleReplay? _prevReplay;
            private int _prevRank;
            private IBattleRoyaleHost _battleRoyaleHost = null!;
            private CancellationTokenSource _tokenSource = new();
            private Task? _refreshColorTask;
            private Task? _refreshPlayerTask;

            protected override void OnInit(BattleRoyaleReplay item) {
                if (item == _prevReplay && item.ReplayRank == _prevRank) {
                    return;
                }
                //
                if (_refreshColorTask != null || _refreshPlayerTask != null) {
                    _tokenSource.Cancel();
                    _tokenSource = new();
                }
                ResetColor();
                RefreshOtherText();
                
                _refreshPlayerTask = RefreshPlayer(_tokenSource.Token).RunCatching();
                _refreshColorTask = RefreshAccentColor(_tokenSource.Token).RunCatching();
                
                _prevReplay = item;
                _prevRank = item.ReplayRank;
            }

            public void Init(IBattleRoyaleHost battleRoyaleHost) {
                _battleRoyaleHost = battleRoyaleHost;
            }

            private async Task RefreshAccentColor(CancellationToken token) {
                var data = await Item.GetBattleRoyaleDataAsync(false, token);
                
                if (token.IsCancellationRequested) {
                    return;
                }
                _refreshColorTask = null;
                //applying
                var color = data.AccentColor ?? Color.white;
                SetColor(color);
            }

            private async Task RefreshPlayer(CancellationToken token) {
                var header = Item.ReplayHeader;
                var player = await header.LoadPlayerAsync(false, token) as IPlayer;
                
                if (token.IsCancellationRequested) {
                    return;
                }
                
                _refreshPlayerTask = null;
                //applying
                _playerAvatar.SetAvatar(player);
                _playerNameText.Text = player.Name;
            }

            private void RefreshOtherText() {
                _rankText.Text = $"#{Item.ReplayRank}";
                var timestamp = Item.ReplayHeader.ReplayInfo.Timestamp.ToString();
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
            cell.Init(_battleRoyaleHost!);
        }

        protected override void OnInitialize() {
            base.OnInitialize();
            SelectionMode = global::Reactive.Components.Basic.SelectionMode.None;
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
            Items.Add(replay);
            Refresh();
        }

        private void HandleReplayRemoved(BattleRoyaleReplay replay, object caller) {
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