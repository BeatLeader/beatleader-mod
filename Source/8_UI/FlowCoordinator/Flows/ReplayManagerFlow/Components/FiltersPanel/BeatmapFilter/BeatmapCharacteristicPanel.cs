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
    internal class BeatmapCharacteristicPanel : ReactiveComponent {
        #region Construct

        public event Action<BeatmapCharacteristicSO>? CharacteristicSelectedEvent;

        private BsSegmentedControl<BeatmapCharacteristicSO?> _segmentedControl = null!;
        private Label _emptyLabel = null!;

        protected override GameObject Construct() {
            return new Layout {
                    FlexItem = {
                        Size = new() { x = 52f, y = 6f },
                        Margin = 0.5f
                    },

                    Children = {
                        new BsSegmentedControl<BeatmapCharacteristicSO?> {
                            FlexController = {
                                Padding = 1f
                            },

                            FlexItem = {
                                FlexGrow = 1f,
                                FlexShrink = 1f,
                                Size = new() { y = 100.pct }
                            },

                            Enabled = false,
                            Items = new Dictionary<BeatmapCharacteristicSO?, BsSegmentedControlItem>(),
                            Key = null,
                            OnKeyChanged = x => CharacteristicSelectedEvent?.Invoke(x!)
                        }.Bind(ref _segmentedControl),

                        new Label {
                            FlexItem = {
                                Position = 0f,
                                PositionType = PositionType.Absolute
                            },

                            Text = "No Characteristics",
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

        public void SetData(IReadOnlyList<BeatmapCharacteristicSO>? characteristics) {
            _emptyLabel.Enabled = characteristics == null;
            _segmentedControl.Enabled = characteristics != null;

            if (characteristics != null) {
                _segmentedControl.Items = characteristics.ToDictionary(
                    static x => (BeatmapCharacteristicSO?)x,
                    static x => new BsSegmentedControlItem(null, x.icon)
                );

                _segmentedControl.Key = characteristics[0];
            }
        }

        #endregion
    }
}