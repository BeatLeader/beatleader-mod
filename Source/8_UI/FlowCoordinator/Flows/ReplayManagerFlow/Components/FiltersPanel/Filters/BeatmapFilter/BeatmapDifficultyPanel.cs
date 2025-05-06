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
    internal class BeatmapDifficultyPanel : ReactiveComponent {
        #region Prefab

        private static Transform SegmentedControlPrefab {
            get {
                TouchPrefab();
                return _segmentedControlPrefab!;
            }
        }

        public static void TouchPrefab() {
            if (_segmentedControlPrefab == null) {
                _segmentedControlPrefab = Resources.FindObjectsOfTypeAll<BeatmapDifficultySegmentedControlController>().First().transform.parent;
            }
        }

        private static Transform? _segmentedControlPrefab;

        #endregion

        #region Construct
        
        public event Action<BeatmapDifficulty>? DifficultySelectedEvent;

        private BeatmapDifficultySegmentedControlController _segmentedControl = null!;
        private Label _emptyLabel = null!;

        protected override GameObject Construct() {
            return new Layout {
                Children = {
                    new GameObjectWrapper(() => {
                            var panel = (RectTransform)UnityEngine.Object.Instantiate(SegmentedControlPrefab);

                            var control = panel.GetComponentInChildren<SegmentedControl>(true);
                            control.SetField("_container", BeatSaberUtils.MenuContainer);

                            _segmentedControl = panel.GetComponentInChildren<BeatmapDifficultySegmentedControlController>(true);

                            return panel.gameObject;
                        }
                    ).AsFlexItem(
                        margin: 0.5f,
                        size: new() { x = 52f, y = 6f }
                    ),

                    new Label {
                        Text = "No Difficulties",
                        Alignment = TextAlignmentOptions.Midline,
                        Color = BeatSaberStyle.SecondaryTextColor
                    }.AsFlexItem(position: 0f).Bind(ref _emptyLabel)
                }
            }.AsFlexGroup().Use();
        }

        protected override void OnInitialize() {
            this.AsFlexItem();
            _segmentedControl.didSelectDifficultyEvent += HandleBeatmapDifficultySelected;
            _difficulties = _segmentedControl._difficulties;
        }

        #endregion

        #region SetData

        private List<BeatmapDifficulty> _difficulties = null!;
        
        public void SetData(IReadOnlyList<BeatmapDifficulty>? difficulties) {
            var empty = difficulties == null;
            _emptyLabel.Enabled = empty;
            _segmentedControl.gameObject.SetActive(!empty);
            
            if (!empty) {
                _segmentedControl.SetData(
                    difficulties,
                    difficulties!.First(),
                    BeatmapDifficultyMask.All
                );
            }

            if (_difficulties.Count > 0) {
                _segmentedControl.HandleDifficultySegmentedControlDidSelectCell(null, 0);
            }
        }

        #endregion

        #region Callbacks

        private void HandleBeatmapDifficultySelected(
            BeatmapDifficultySegmentedControlController self,
            BeatmapDifficulty beatmapDifficulty
        ) {
            DifficultySelectedEvent?.Invoke(beatmapDifficulty);
        }

        #endregion
    }
}