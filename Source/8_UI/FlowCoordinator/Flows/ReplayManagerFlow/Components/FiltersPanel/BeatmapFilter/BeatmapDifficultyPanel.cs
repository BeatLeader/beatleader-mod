using System;
using System.Collections.Generic;
using System.Linq;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BeatmapDifficultyPanel : ReactiveComponent {
        #region Construct
        
        public event Action<BeatmapDifficulty>? DifficultySelectedEvent;

        private BsSegmentedControl<BeatmapDifficulty?> _segmentedControl = null!;
        private Label _emptyLabel = null!;

        protected override GameObject Construct() {
            return new Layout {
                    FlexItem = {
                        Size = new() { x = 52f, y = 6f },
                        Margin = 0.5f
                    },

                    Children = {
                        new BsSegmentedControl<BeatmapDifficulty?> {
                            FlexController = {
                                Padding = 1f
                            },

                            FlexItem = {
                                FlexGrow = 1f,
                                FlexShrink = 1f,
                                Size = new() { y = 100.pct }
                            },

                            Enabled = false,
                            Items = new Dictionary<BeatmapDifficulty?, BsSegmentedControlItem>(),
                            Key = null,
                            OnKeyChanged = x => DifficultySelectedEvent?.Invoke(x!.Value)
                        }.Bind(ref _segmentedControl),

                        new Label {
                            FlexItem = {
                                Position = 0f,
                                PositionType = PositionType.Absolute
                            },

                            Text = "No Difficulties",
                            Alignment = TextAlignmentOptions.Midline,
                            Color = BeatSaberStyle.SecondaryTextColor
                        }.Bind(ref _emptyLabel)
                    }
                }
                .AsFlexGroup()
                .Use();
        }

        #endregion

        #region SetData

        public void SetData(IReadOnlyList<BeatmapDifficulty>? difficulties) {
            _emptyLabel.Enabled = difficulties == null;
            _segmentedControl.Enabled = difficulties != null;

            if (difficulties != null) {
                _segmentedControl.Items = difficulties.ToDictionary(
                    static x => (BeatmapDifficulty?)x,
                    static x => new BsSegmentedControlItem(x.Name(), null)
                );

                _segmentedControl.Key = difficulties[0];
            }
        }

        #endregion
    }
}