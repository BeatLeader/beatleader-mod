using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace BeatLeader.Components {
    internal class BeatmapDifficultyPanel : ReeUIComponentV2 {
        #region Prefab

        private static Transform BeatmapDifficultySegmentedControl => _beatmapDifficultySegmentedControl ?
            _beatmapDifficultySegmentedControl! : _beatmapDifficultySegmentedControl = Resources
                .FindObjectsOfTypeAll<BeatmapDifficultySegmentedControlController>().First().transform.parent;

        private static Transform? _beatmapDifficultySegmentedControl;

        #endregion

        #region Events

        public event Action<BeatmapDifficulty>? DifficultySelectedEvent;

        #endregion

        #region UI Components

        [UIComponent("container")]
        private readonly Transform _container = null!;

        [UIObject("text-container")]
        private readonly GameObject _text = null!;

        private BeatmapDifficultySegmentedControlController? _segmentedControl;
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

        public void SetData(IReadOnlyList<IDifficultyBeatmap>? difficulties) {
            if (_segmentedControl is null) throw new UninitializedComponentException();
            var empty = difficulties is null;
            _text.SetActive(empty);
            _segmentedControl.gameObject.SetActive(!empty);
            if (!empty) _segmentedControl.SetData(
                difficulties, difficulties!.First().difficulty);
            RefreshTouchables();
            RefreshActive();
            if (_difficulties.Count > 0) {
                _segmentedControl.HandleDifficultySegmentedControlDidSelectCell(null, 0);
            }
        }

        #endregion
        
        #region Init

        private List<BeatmapDifficulty> _difficulties = null!;
        
        protected override void OnInitialize() {
            var characteristicPanel = Instantiate(
                BeatmapDifficultySegmentedControl, _container, true);
            var panelTransform = characteristicPanel.transform;
            panelTransform.localScale = Vector3.one;
            panelTransform.localPosition = Vector3.one;
            characteristicPanel
                .GetComponentInChildren<TextSegmentedControl>(true)
                .SetField("_container", BeatSaberUI.DiContainer);
            _canvasGroup = _container.gameObject.AddComponent<CanvasGroup>();
            _segmentedControl = characteristicPanel.GetComponentInChildren<BeatmapDifficultySegmentedControlController>(true);
            _difficulties = _segmentedControl.GetField<List<BeatmapDifficulty>, BeatmapDifficultySegmentedControlController>("_difficulties");
            _segmentedControl.didSelectDifficultyEvent += HandleBeatmapDifficultySelected;
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