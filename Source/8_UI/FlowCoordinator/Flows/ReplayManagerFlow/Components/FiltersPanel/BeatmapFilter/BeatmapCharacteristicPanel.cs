using System;
using System.Collections.Generic;
using System.Linq;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using TMPro;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class BeatmapCharacteristicPanel : ReactiveComponent {
        #region Construct

        public event Action<BeatmapCharacteristicSO>? CharacteristicSelectedEvent;

        private IconSegmentedControl<BeatmapCharacteristicSO> _segmentedControl = null!;
        private Label _emptyLabel = null!;

        protected override GameObject Construct() {
            return new Background {
                    Children = {
                        new IconSegmentedControl<BeatmapCharacteristicSO> {
                            KeySelectedCb = x => CharacteristicSelectedEvent?.Invoke(x),
                            CellSpawnedCb = (_, cell) => {
                                cell.Colors = BeatSaberStyle.TextColorSet.With(
                                    color: Color.white.ColorWithAlpha(0.9f),
                                    hoveredColor: Color.white
                                );

                                cell.Image.Material = global::Reactive.BeatSaber.GameResources.UINoGlowMaterial;
                                cell.Image.Skew = BeatSaberStyle.Skew;
                            }
                        }.AsFlexGroup(padding: 1f).Bind(ref _segmentedControl),

                        new Label {
                            Text = "No Characteristics",
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

        public void SetData(IEnumerable<BeatmapCharacteristicSO>? beatmapCharacteristics) {
            var empty = beatmapCharacteristics == null;
            _emptyLabel.Enabled = empty;
            _segmentedControl.Enabled = !empty;

            if (!empty) {
                var items = _segmentedControl.Items;
                items.Clear();

                // ReSharper disable PossibleMultipleEnumeration
                foreach (var characteristic in beatmapCharacteristics!) {
                    items.Add(characteristic, characteristic.icon);
                }

                _segmentedControl.Select(beatmapCharacteristics.First());
            }
        }

        #endregion
    }
}