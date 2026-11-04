using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplaysPreviewPanel : ReactiveComponent {
        #region Replays

        public ICollection<IReplayHeader> Replays => _replays;

        private ObservableSet<IReplayHeader> _replays = null!;

        private void HandleReplayAddedOrRemoved(IReplayHeader header) {
            RefreshReplaysLabel();
            RefreshAvatarPlacement();
        }

        private void RefreshReplaysLabel() {
            _replaysLabel.Text = $"{_replays.Count} REPLAYS";
        }

        #endregion

        #region Avatars

        private class Avatar : ReeWrapperV2<Components.PlayerAvatar> {
            private CancellationTokenSource _tokenSource = new();
            private Task? _setAvatarTask;

            public void SetAvatar(IReplayHeader replay) {
                if (_setAvatarTask != null) {
                    _tokenSource.Cancel();
                    _tokenSource = new();
                }
                _setAvatarTask = SetAvatarInternal(replay, _tokenSource.Token);
            }

            private async Task SetAvatarInternal(IReplayHeader replay, CancellationToken token) {
                var player = await replay.LoadPlayerAsync(false, token) as IPlayer;
                
                if (token.IsCancellationRequested) {
                    return;
                }
                
                ReeComponent.SetAvatar(player.AvatarUrl, null);
                _setAvatarTask = null;
            }
        }

        public int MaxAvatarCount {
            get => _maxAvatarCount;
            set {
                _maxAvatarCount = value;
                RefreshAvatarPlacement();
            }
        }

        private readonly ReactivePool<IReplayHeader, Avatar> _avatarsPool = new();
        private int _maxAvatarCount = 5;
        private float _avatarSize = 6f;

        private void RefreshAvatarPlacement() {
            _avatarsPool.DespawnAll();
            _avatarsContainer.Children.Clear();
            
            var lastIndex = Mathf.Min(_replays.Count, _maxAvatarCount);
            _avatarsContainer.FlexItem.Size = new() { x = CalculateIndent(lastIndex) };
            
            var index = 0;
            foreach (var replay in _replays) {
                if (index >= _maxAvatarCount) {
                    return;
                }
                
                var wrapper = _avatarsPool.Spawn(replay);
                var flexItem = wrapper.FlexItem;
                flexItem.AspectRatio = 1f;
                flexItem.Size = new() { x = _avatarSize };
                flexItem.Position = new() { left = CalculateIndent(index) };
                flexItem.PositionType = PositionType.Absolute;
                
                _avatarsContainer.Children.Add(wrapper);
                
                var avatar = wrapper.ReeComponent;
                avatar.Setup(true);
                wrapper.SetAvatar(replay);
                
                index++;
            }
        }

        protected override void OnEnable() {
            RefreshAvatarPlacement();
        }

        private float CalculateIndent(int index) {
            return index * _avatarSize * 0.6f;
        }

        #endregion

        #region Construct

        private Layout _avatarsContainer = null!;
        private Label _replaysLabel = null!;

        protected override GameObject Construct() {
            return new Layout {
                FlexController = {
                    JustifyContent = Justify.Center,
                    AlignItems = Align.Center,
                    Gap = 1f
                },

                Children = {
                    new Layout {
                        FlexController = {
                            Padding = 2f
                        },

                        FlexItem = {
                            Size = new() { y = 100.pct }
                        },

                        Children = {
                            new Layout().AsFlexGroup().Bind(ref _avatarsContainer)
                        }
                    },
                    //
                    new Label {
                        FlexItem = {
                            Size = YogaVector.Auto
                        },

                        FontSize = 5f,
                        FontStyle = FontStyles.Italic
                    }.Bind(ref _replaysLabel)
                }
            }.Use();
        }

        protected override void OnInitialize() {
            _replays = new(
                HandleReplayAddedOrRemoved,
                HandleReplayAddedOrRemoved
            );
        }

        #endregion
    }
}