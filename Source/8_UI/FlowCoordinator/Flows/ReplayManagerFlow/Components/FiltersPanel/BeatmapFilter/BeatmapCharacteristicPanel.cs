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
        #region Setup

        public IReadOnlyList<BeatmapCharacteristicSO>? Characteristics {
            get => _characteristics.Value;
            set => _characteristics.Value = value;
        }

        public Action<BeatmapCharacteristicSO>? OnCharacteristicSelected { get; set; }

        #endregion

        #region Construct

        private State<IReadOnlyList<BeatmapCharacteristicSO>?> _characteristics = null!;

        protected override GameObject Construct() {
            _characteristics = Remember<IReadOnlyList<BeatmapCharacteristicSO>?>(null);

            return new Layout {
                FlexItem = {
                    Size = new() { y = 5.pt }
                },

                Children = {
                    new BsSegmentedControl<BeatmapCharacteristicSO?> {
                        FlexItem = {
                            FlexGrow = 1f,
                            FlexShrink = 1f,
                            Size = new() { y = 5.pt }
                        },

                        Skew = 0f,
                        
                        sEnabled = _characteristics.Map(static x => x != null),
                        sKey = _characteristics.Map(static x => x?.Count > 0 ? x[0] : null),
                        
                        sItems = _characteristics.Map(static list => (list ?? []).ToDictionary(
                            static x => (BeatmapCharacteristicSO?)x,
                            static x => new BsSegmentedControlItem(null, x.icon))
                        ),

                        OnKeyChanged = x => {
                            if (x != null) OnCharacteristicSelected?.Invoke(x);
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
                        sEnabled = _characteristics.Map(static x => x == null)
                    }
                }
            }.AsFlexGroup().Use();
        }

        #endregion
    }
}