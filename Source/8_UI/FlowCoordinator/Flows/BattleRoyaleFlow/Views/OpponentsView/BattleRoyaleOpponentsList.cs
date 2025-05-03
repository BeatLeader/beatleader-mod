using System.Collections.Generic;
using System.Diagnostics;
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
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleOpponentsList : Table<BattleRoyaleReplay, BattleRoyaleOpponentsList.Cell> {
        #region Cells

        public class Cell : TableCell<BattleRoyaleReplay> {
            #region Construct

            private Image _backgroundImage = null!;
            private PlayerAvatar _playerAvatar = null!;
            private Label _playerNameText = null!;
            private Label _rankText = null!;
            private Label _dateText = null!;
            private BsButtonBase _removeButton = null!;
            private BsButtonBase _navigateButton = null!;

            protected override GameObject Construct() {
                return new Layout {
                    Children = {
                        new Background {
                            Sprite = BundleLoader.Sprites.background,
                            Color = Color.white.ColorWithAlpha(0.2f),
                            PixelsPerUnit = 8f,
                            Skew = BeatSaberStyle.Skew,

                            Children = {
                                //rank
                                new Label {
                                    LayoutModifier = new YogaModifier {
                                        Margin = new() { left = 1.pt(), right = 1.pt() },
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
                                new ImageBsButton {
                                    LayoutModifier = new YogaModifier {
                                        Size = new() { x = 6.pt() }
                                    },

                                    Sprite = BundleLoader.Sprites.crossIcon,
                                    ShowUnderline = false,
                                    OnClick = HandleRemoveButtonClicked
                                }.Bind(ref _removeButton),

                                //navigate button
                                new ImageBsButton {
                                    LayoutModifier = new YogaModifier {
                                        Size = new() { x = 6.pt() }
                                    },

                                    Sprite = BundleLoader.Sprites.rightArrowIcon,
                                    ShowUnderline = false,
                                    OnClick = HandleNavigateButtonClicked
                                }.Bind(ref _navigateButton)
                            }
                        }.AsFlexGroup(
                            padding: 1f,
                            gap: 1f
                        ).AsFlexItem(
                            flexGrow: 1f,
                            margin: new() { right = 1f, left = 1f }
                        ).Bind(ref _backgroundImage)
                    }
                }.AsFlexGroup().AsFlexItem(size: new() { y = 10f }).Use();
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

                if (_refreshColorTask?.Status is TaskStatus.Running || _refreshPlayerTask?.Status is TaskStatus.Running) {
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
                _removeButton.Enabled = battleRoyaleHost.CanMutateLobby;
                _navigateButton.Enabled = battleRoyaleHost.CanMutateLobby;
            }

            private async Task RefreshAccentColor(CancellationToken token) {
                var data = await Item.GetBattleRoyaleDataAsync(false, token);

                if (token.IsCancellationRequested) {
                    return;
                }

                var color = data.AccentColor ?? Color.white;
                SetColor(color);
            }

            private async Task RefreshPlayer(CancellationToken token) {
                var header = Item.ReplayHeader;
                var player = await header.LoadPlayerAsync(false, token) as IPlayer;

                if (token.IsCancellationRequested) {
                    return;
                }

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

        private void HandleCanMutateLobbyChangedEvent(bool canMutate) {
            Refresh();
        }

        #endregion
    }
}