﻿using System;
using System.Collections.Generic;
using System.Linq;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Components {
    internal class BeatmapDifficultyPanel : ReeUIComponentV2 {
        #region Prefab

        public static Transform BeatmapDifficultySegmentedControl => _beatmapDifficultySegmentedControl ?
            _beatmapDifficultySegmentedControl! : _beatmapDifficultySegmentedControl = Instantiate(Resources
                .FindObjectsOfTypeAll<BeatmapDifficultySegmentedControlController>().First().transform.parent);

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

        public void SetData(IReadOnlyList<BeatmapDifficulty>? difficulties) {
            if (_segmentedControl is null) throw new UninitializedComponentException();
            var empty = difficulties is null;
            _text.SetActive(empty);
            _segmentedControl.gameObject.SetActive(!empty);
            if (!empty) {
                _segmentedControl.SetData(
                    difficulties,
                    difficulties!.First(),
                    BeatmapDifficultyMask.All
                );
            }
            RefreshTouchables();
            RefreshActive();
        }

        #endregion

        #region Init

        protected override void OnInitialize() {
            var characteristicPanel = BeatmapDifficultySegmentedControl;
            characteristicPanel.SetParent(_container, true);
            characteristicPanel.localScale = Vector3.one;
            characteristicPanel.localPosition = Vector3.zero;
            characteristicPanel
                .GetComponentInChildren<SegmentedControl>(true)
                .SetField("_container", BeatSaberUI.DiContainer);
            _canvasGroup = _container.gameObject.AddComponent<CanvasGroup>();
            _segmentedControl = characteristicPanel.GetComponentInChildren<BeatmapDifficultySegmentedControlController>(true);
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