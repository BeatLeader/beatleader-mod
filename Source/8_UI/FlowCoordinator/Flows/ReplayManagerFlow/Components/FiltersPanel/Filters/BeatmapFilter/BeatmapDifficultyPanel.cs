using System;
using System.Collections.Generic;
using System.Linq;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BeatmapDifficultyPanel : ReactiveComponent {
        #region Construct
        
        public event Action<BeatmapDifficulty>? DifficultySelectedEvent;

        private TextSegmentedControl<BeatmapDifficulty> _segmentedControl = null!;
        private Label _emptyLabel = null!;

        protected override GameObject Construct() {
            return new Background {
                    Children = {
                        new TextSegmentedControl<BeatmapDifficulty> {
                            KeySelectedCb = x => DifficultySelectedEvent?.Invoke(x),
                            CellSpawnedCb = (_, cell) => {
                                cell.Colors = BeatSaberStyle.TextColorSet.With(
                                    color: Color.white.ColorWithAlpha(0.9f),
                                    hoveredColor: Color.white
                                );

                                cell.Alignment = TextAlignmentOptions.Capline;
                                cell.FontSize = 3.5f;
                                cell.EnableAutoSizing = false;
                                cell.FontStyle |= FontStyles.Italic;
                            }
                        }.AsFlexGroup(padding: 1f).Bind(ref _segmentedControl),

                        new Label {
                            Text = "No Difficulties",
                            Alignment = TextAlignmentOptions.Midline,
                            Color = BeatSaberStyle.SecondaryTextColor
                        }.AsFlexItem(position: 0f).Bind(ref _emptyLabel)
                    }
                }
                .AsBlurBackground(color: Color.black.ColorWithAlpha(0.55f))
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
            var empty = difficulties == null;
            _emptyLabel.Enabled = empty;
            _segmentedControl.Enabled = !empty;
            
            if (!empty) {
                var items = _segmentedControl.Items;
                items.Clear();

                // ReSharper disable PossibleMultipleEnumeration
                foreach (var characteristic in difficulties!) {
                    items.Add(characteristic, characteristic.Name());
                }

                _segmentedControl.Select(difficulties.First());
            }
        }

        #endregion
    }
}