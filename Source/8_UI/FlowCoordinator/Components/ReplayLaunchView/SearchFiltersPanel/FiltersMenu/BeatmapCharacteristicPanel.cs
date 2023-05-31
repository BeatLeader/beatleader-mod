using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace BeatLeader.Components {
    internal class BeatmapCharacteristicPanel : ReeUIComponentV2 {
        #region Prefab

        private static Transform BeatmapCharacteristicSegmentedControl => _beatmapCharacteristicSegmentedControl ?
            _beatmapCharacteristicSegmentedControl! : _beatmapCharacteristicSegmentedControl = Resources
                .FindObjectsOfTypeAll<BeatmapCharacteristicSegmentedControlController>().First().transform.parent;

        private static Transform? _beatmapCharacteristicSegmentedControl;

        #endregion

        #region Events

        public event Action<BeatmapCharacteristicSO>? CharacteristicSelectedEvent;

        #endregion

        #region UI Components

        [UIComponent("container")]
        private readonly Transform _container = null!;

        [UIObject("text-container")]
        private readonly GameObject _text = null!;

        private BeatmapCharacteristicSegmentedControlController? _segmentedControl;
        private CanvasGroup? _canvasGroup;
        private IEnumerable<Touchable>? _touchables;

        #endregion

        #region SetActive

        private bool _isActive = true;

        public void SetActive(bool active) {
            if (_segmentedControl is null) throw new UninitializedComponentException();
            _isActive = active;
            RefreshActive();
        }

        private void RefreshActive() {
            _canvasGroup!.alpha = _isActive ? 1f : 0.6f;
            if (_touchables is null) return;
            foreach (var touchable in _touchables) {
                if (!touchable) continue;
                touchable.enabled = _isActive;
            }
        }

        private void RefreshTouchables() {
            _touchables = _segmentedControl!.GetComponentsInChildren<Touchable>(true);
        }

        #endregion

        #region SetData

        public void SetData(IReadOnlyList<IDifficultyBeatmapSet>? difficultyBeatmapSets) {
            if (_segmentedControl is null) throw new UninitializedComponentException();
            var empty = difficultyBeatmapSets is null;
            _text.SetActive(empty);
            _segmentedControl.gameObject.SetActive(!empty);
            if (!empty) _segmentedControl.SetData(difficultyBeatmapSets, 
                difficultyBeatmapSets!.First().beatmapCharacteristic);
            RefreshTouchables();
            RefreshActive();
            _segmentedControl.HandleDifficultySegmentedControlDidSelectCell(null, 0);
        }

        #endregion
        
        #region Init

        protected override void OnInitialize() {
            var characteristicPanel = Instantiate(
                BeatmapCharacteristicSegmentedControl, _container, true);
            characteristicPanel
                .GetComponentInChildren<IconSegmentedControl>(true)
                .SetField("_container", BeatSaberUI.DiContainer);
            _canvasGroup = _container.gameObject.AddComponent<CanvasGroup>();
            _segmentedControl = characteristicPanel.GetComponentInChildren<BeatmapCharacteristicSegmentedControlController>(true);
            _segmentedControl.didSelectBeatmapCharacteristicEvent += HandleBeatmapCharacteristicSelected;
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