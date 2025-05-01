using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Components;
using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleReplayBadge : ReactiveComponent {
        #region Setup

        public async Task SetData(BattleRoyaleReplay replay) {
            Hide();

            var header = replay.ReplayHeader;
            var player = await header.LoadPlayerAsync(false, CancellationToken.None);
            var timestamp = header.ReplayInfo.Timestamp.ToString();

            _rankLabel.Text = $"#{replay.ReplayRank}";
            _nameLabel.Text = player.name;
            _playerAvatar.SetAvatar(player);
            _dateLabel.Text = FormatUtils.FormatTimeset(timestamp, true);

            Show();
        }

        #endregion

        #region Animation

        private AnimatedValue<Vector3> _scale = null!;

        private void Show() {
            _scale.Value = Vector3.one;
        }

        private void Hide() {
            _scale.SetValueImmediate(Vector3.zero);
        }

        #endregion

        #region Construct

        private PlayerAvatar _playerAvatar = null!;
        private Label _rankLabel = null!;
        private Label _dateLabel = null!;
        private Label _nameLabel = null!;

        protected override GameObject Construct() {
            _scale = RememberAnimated(Vector3.zero, 10.fact());

            return new Background {
                Children = {
                    new Layout {
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
            }.With(
                x => {
                    x.AsBlurBackground();
                    x.Animate(_scale, static (x, y) => x.ContentTransform.localScale = y);
                    x.AsFlexGroup(
                        direction: FlexDirection.Column,
                        padding: new() { top = 1f, bottom = 0.3f, left = 2f, right = 2f },
                        alignItems: Align.Center,
                        gap: 0.5f,
                        constrainHorizontal: false,
                        constrainVertical: false
                    );
                    x.AsFlexItem(size: "auto");
                }
            ).Use();
        }

        protected override void OnInitialize() {
            Hide();
        }

        #endregion
    }
}