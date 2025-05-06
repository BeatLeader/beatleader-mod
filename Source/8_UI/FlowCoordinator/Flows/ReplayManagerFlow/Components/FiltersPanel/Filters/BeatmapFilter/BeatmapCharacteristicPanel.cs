using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.UI.Reactive.Components;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using Reactive;
using Reactive.BeatSaber;
using Reactive.BeatSaber.Components;
using TMPro;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BeatmapCharacteristicPanel : ReactiveComponent {
        #region Prefab

        private static Transform SegmentedControlPrefab {
            get {
                TouchPrefab();
                return _segmentedControlPrefab!;
            }
        }

        public static void TouchPrefab() {
            if (_segmentedControlPrefab == null) {
                _segmentedControlPrefab = Resources.FindObjectsOfTypeAll<BeatmapCharacteristicSegmentedControlController>().First().transform.parent;
            }
        }

        private static Transform? _segmentedControlPrefab;

        #endregion

        #region Construct

        public event Action<BeatmapCharacteristicSO>? CharacteristicSelectedEvent;

        private BeatmapCharacteristicSegmentedControlController _segmentedControl = null!;
        private Label _emptyLabel = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new GameObjectWrapper(() => {
                            var panel = (RectTransform)UnityEngine.Object.Instantiate(SegmentedControlPrefab);

                            var control = panel.GetComponentInChildren<SegmentedControl>(true);
                            control.SetField("_container", BeatSaberUtils.MenuContainer);

                            _segmentedControl = panel.GetComponentInChildren<BeatmapCharacteristicSegmentedControlController>(true);

                            return panel.gameObject;
                        }
                    ).AsFlexItem(
                        margin: 0.5f,
                        size: new() { x = 52f, y = 6f }
                    ),

                    new Label {
                        Text = "No Characteristics",
                        Alignment = TextAlignmentOptions.Midline,
                        Color = BeatSaberStyle.SecondaryTextColor
                    }.AsFlexItem(position: 0f).Bind(ref _emptyLabel)
                }
            }.AsFlexGroup().Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem();
            _segmentedControl.didSelectBeatmapCharacteristicEvent += HandleBeatmapCharacteristicSelected;
            _beatmapCharacteristics = _segmentedControl._currentlyAvailableBeatmapCharacteristics;
        }

        #endregion

        #region SetData

        private static readonly HashSet<BeatmapCharacteristicSO> notAllowedCharacteristics = new(0);
        private List<BeatmapCharacteristicSO> _beatmapCharacteristics = null!;

        public void SetData(IEnumerable<BeatmapCharacteristicSO>? beatmapCharacteristics) {
            var empty = beatmapCharacteristics == null;
            _emptyLabel.Enabled = empty;
            _segmentedControl.gameObject.SetActive(!empty);

            if (!empty) {
                // ReSharper disable PossibleMultipleEnumeration
                _segmentedControl.SetData(
                    beatmapCharacteristics,
                    beatmapCharacteristics!.First(),
                    notAllowedCharacteristics
                );
            }

            if (_beatmapCharacteristics?.Count > 0) {
                _segmentedControl.HandleBeatmapCharacteristicSegmentedControlDidSelectCell(null, 0);
            }
        }

        #endregion

        #region Callbacks

        private void HandleBeatmapCharacteristicSelected(
            BeatmapCharacteristicSegmentedControlController self,
            BeatmapCharacteristicSO characteristic
        ) {
            CharacteristicSelectedEvent?.Invoke(characteristic);
        }

        #endregion
    }
}