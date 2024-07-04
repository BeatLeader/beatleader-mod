using System.Collections.Generic;
using System.Threading;
using BeatLeader.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReplaysPreviewPanel : ReactiveComponent {
        #region Replays

        public ICollection<IReplayHeaderBase> Replays => _replays;

        private ObservableSet<IReplayHeaderBase> _replays = null!;

        private void HandleReplayAddedOrRemoved(IReplayHeaderBase header) {
            RefreshReplaysLabel();
            RefreshAvatarPlacement();
        }

        private void RefreshReplaysLabel() {
            _replaysLabel.Text = $"{_replays.Count} REPLAYS";
        }

        #endregion

        #region Avatars

        private class Avatar : ReeWrapperV2<BeatLeader.Components.PlayerAvatar> { }

        public int MaxAvatarCount {
            get => _maxAvatarCount;
            set {
                _maxAvatarCount = value;
                RefreshAvatarPlacement();
            }
        }

        private readonly ReactivePool<IReplayHeaderBase, Avatar> _avatarsPool = new();
        private readonly SemaphoreSlim _refreshAvatarSemaphore = new(1, 1);
        private int _maxAvatarCount = 5;
        private float _avatarSize;

        private async void RefreshAvatarPlacement() {
            await _refreshAvatarSemaphore.WaitAsync();
            _avatarsPool.DespawnAll();
            _avatarsContainer.Children.Clear();
            //uncomment for adaptivity
            //_avatarSize = _avatarsContainer.ContentTransform.rect.height;
            _avatarSize = 6f;
            var lastIndex = Mathf.Min(_replays.Count, _maxAvatarCount);
            _avatarsContainerModifier.Size = new() { x = CalculateIndent(lastIndex) };
            //
            var index = 0;
            foreach (var replay in _replays) {
                if (index >= _maxAvatarCount) return;
                //
                var playerTask = replay.LoadPlayerAsync(false, CancellationToken.None);
                var wrapper = _avatarsPool.Spawn(replay);
                wrapper.AsFlexItem(
                    aspectRatio: 1f,
                    size: new() { x = _avatarSize },
                    position: new() { left = CalculateIndent(index) }
                );
                _avatarsContainer.Children.Add(wrapper);
                //
                var avatar = wrapper.ReeComponent;
                avatar.Setup(true);
                avatar.SetAvatar((await playerTask).AvatarUrl, null);
                //
                index++;
            }
            //
            _refreshAvatarSemaphore.Release();
        }

        protected override void OnLayoutApply() {
            RefreshAvatarPlacement();
        }

        protected override void OnEnable() {
            RefreshAvatarPlacement();
        }

        private float CalculateIndent(int index) {
            return index * _avatarSize * 0.6f;
        }

        #endregion

        #region Construct

        private YogaModifier _avatarsContainerModifier = null!;
        private Dummy _avatarsContainer = null!;
        private Label _replaysLabel = null!;

        protected override GameObject Construct() {
            return new Dummy {
                Children = {
                    new Dummy {
                        Children = {
                            new Dummy()
                                .AsFlexGroup()
                                .AsFlexItem(modifier: out _avatarsContainerModifier)
                                .Bind(ref _avatarsContainer)
                        }
                    }.AsFlexGroup(padding: 2f).AsFlexItem(size: new() { y = "100%" }),
                    //
                    new Label {
                            FontSize = 5f,
                            FontStyle = FontStyles.Italic
                        }
                        .AsFlexItem(size: "auto")
                        .Bind(ref _replaysLabel)
                }
            }.AsFlexGroup(
                justifyContent: Justify.Center,
                alignItems: Align.Center,
                gap: 1f
            ).Use();
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