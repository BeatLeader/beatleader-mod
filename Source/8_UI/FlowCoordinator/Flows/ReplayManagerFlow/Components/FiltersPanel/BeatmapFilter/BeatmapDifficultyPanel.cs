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
                    Children = {
                        new BsSegmentedControl<BeatmapDifficulty?> {
                                Enabled = false,
                                Items = new Dictionary<BeatmapDifficulty?, BsSegmentedControlItem>(),
                                Key = null,
                                OnKeyChanged = x => DifficultySelectedEvent?.Invoke(x!.Value)
                            }
                            .AsFlexGroup(padding: 1f)
                            .AsFlexItem(flexGrow: 1f, flexShrink: 1f, size: new() { y = 100.pct })
                            .Bind(ref _segmentedControl),

                        new Label {
                            Text = "No Difficulties",
                            Alignment = TextAlignmentOptions.Midline,
                            Color = BeatSaberStyle.SecondaryTextColor
                        }.AsFlexItem(position: 0f).Bind(ref _emptyLabel)
                    }
                }
                .AsFlexGroup()
                .AsFlexItem(
                    size: new() { x = 52f, y = 6f },
                    margin: 0.5f
                )
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