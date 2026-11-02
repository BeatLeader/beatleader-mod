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

        private AnimatedState<Vector3> _scale = null!;

        private void Show() {
            _scale.TargetValue = Vector3.one;
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
            _scale = RememberAnimated(Vector3.zero, 10.fact);

            return new Background {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    Padding = new() { top = 1f, bottom = 0.3f, left = 2f, right = 2f },
                    AlignItems = Align.Center,
                    Gap = 0.5f,
                    ConstrainHorizontal = false,
                    ConstrainVertical = false
                },

                FlexItem = {
                    Size = "auto"
                },

                ContentTransform = {
                    slocalScale = _scale
                },

                Children = {
                    new Layout {
                        FlexController = {
                            Gap = 1f
                        },

                        FlexItem = {
                            Size = new() { y = 4f }
                        },

                        Children = {
                            new Label {
                                FlexItem = {
                                    Size = "auto"
                                },

                                FontSize = 3f
                            }.Bind(ref _rankLabel),
                            //
                            new ReeWrapperV2<PlayerAvatar> {
                                FlexItem = {
                                    AspectRatio = 1f
                                }
                            }.BindRee(ref _playerAvatar),
                            //
                            new Label {
                                FlexItem = {
                                    Size = "auto"
                                },

                                FontSize = 3f
                            }.Bind(ref _nameLabel)
                        }
                    },
                    //date
                    new Label {
                        FlexItem = {
                            Size = "auto"
                        },

                        Color = UIStyle.SecondaryTextColor,
                        FontSize = 2.8f
                    }.Bind(ref _dateLabel)
                }
            }.AsBlurBackground().Use();
        }

        protected override void OnInitialize() {
            Hide();
        }

        #endregion
    }
}