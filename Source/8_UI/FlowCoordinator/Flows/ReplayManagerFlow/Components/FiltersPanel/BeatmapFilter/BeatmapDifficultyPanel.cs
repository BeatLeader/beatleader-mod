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
        #region Setup

        public IReadOnlyList<BeatmapDifficulty>? Difficulties {
            get => _difficulties.Value;
            set => _difficulties.Value = value;
        }

        public Action<BeatmapDifficulty>? OnDifficultySelected { get; set; }

        #endregion

        #region Construct

        private State<IReadOnlyList<BeatmapDifficulty>?> _difficulties = null!;

        protected override GameObject Construct() {
            _difficulties = Remember<IReadOnlyList<BeatmapDifficulty>?>(null);

            return new Layout {
                    FlexItem = {
                        Size = new() { y = 5.pt }
                    },

                    Children = {
                        new BsSegmentedControl<BeatmapDifficulty?> {
                            FlexItem = {
                                FlexGrow = 1f,
                                FlexShrink = 1f,
                                Size = new() { y = 5.pt }
                            },

                            Skew = 0f,
                            
                            sEnabled = _difficulties.Map(static x => x != null),
                            sKey = _difficulties.Map(static x => x?.Count > 0 ? x[0] : (BeatmapDifficulty?)null),
                            
                            sItems = _difficulties.Map(static list => (list ?? []).ToDictionary(
                                static x => (BeatmapDifficulty?)x,
                                static x => new BsSegmentedControlItem(x.Name(), null))
                            ),
                            
                            OnKeyChanged = x => {
                                if (x.HasValue) OnDifficultySelected?.Invoke(x.Value);
                            }
                        },

                        new Label {
                            FlexItem = {
                                Position = 0f,
                                PositionType = PositionType.Absolute
                            },

                            Text = "Select a map first",
                            FontSize = 3f,
                            Alignment = TextAlignmentOptions.Midline,
                            Color = BeatSaberStyle.SecondaryTextColor,
                            sEnabled = _difficulties.Map(static x => x == null)
                        }
                    }
                }
                .AsFlexGroup()
                .Use();
        }

        #endregion
    }
}