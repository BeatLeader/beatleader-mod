using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Components;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using UnityEngine;
using Dummy = BeatLeader.UI.Reactive.Components.Dummy;
using FlexDirection = BeatLeader.UI.Reactive.Yoga.FlexDirection;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleReplayBadge : ReactiveComponent {
        #region Setup

        public async Task SetData(IBattleRoyaleQueuedReplay replay) {
            var header = replay.ReplayHeader;
            var player = await header.LoadPlayerAsync(false, CancellationToken.None);
            var timestamp = header.ReplayInfo.Timestamp.ToString();
            _rankLabel.Text = $"#{replay.ReplayRank}";
            _nameLabel.Text = player.Name;
            _playerAvatar.SetAvatar(player);
            _dateLabel.Text = FormatUtils.FormatTimeset(timestamp, true);
        }

        #endregion

        #region Construct

        private PlayerAvatar _playerAvatar = null!;
        private Label _rankLabel = null!;
        private Label _dateLabel = null!;
        private Label _nameLabel = null!;

        protected override GameObject Construct() {
            return new Image {
                Children = {
                    new Dummy {
                        Children = {
                            new Label {
                                FontSize = 3f
                            }.AsFlexItem(size: "auto").Bind(ref _rankLabel),
                            //
                            new ReeWrapperV2<PlayerAvatar>()
                                .AsFlexItem(aspectRatio: 1f)
                                .BindRee(ref _playerAvatar),
                            //
                            new Label {
                                FontSize = 3f
                            }.AsFlexItem(size: "auto").Bind(ref _nameLabel)
                        }
                    }.AsFlexGroup(gap: 1f).AsFlexItem(size: new() { y = 4f }),
                    //date
                    new Label {
                        Color = UIStyle.SecondaryTextColor,
                        FontSize = 2.8f
                    }.AsFlexItem(size: "auto").Bind(ref _dateLabel)
                }
            }.AsBlurBackground().AsRootFlexGroup(
                direction: FlexDirection.Column,
                padding: new() { top = 1f, bottom = 0.3f, left = 2f, right = 2f },
                alignItems: Align.Center,
                gap: 0.5f
            ).AsFlexItem(size: "auto").Use();
        }

        #endregion
    }
}