using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class BeatmapSelector : ReeUIComponentV2 {
        #region Events

        public event Action<IPreviewBeatmapLevel?>? BeatmapSelectedEvent;

        #endregion

        #region UI Components

        [UIObject("tab-selector")]
        private readonly GameObject _tabSelectorObject = null!;

        [UIObject("container")]
        private readonly GameObject _containerObject = null!;

        [UIValue("beatmap-preview"), UsedImplicitly]
        private BeatmapPreviewCell _beatmapPreview = null!;

        private CanvasGroup? _canvasGroup;
        private IEnumerable<Touchable>? _touchables;

        private static LevelSelectionNavigationController? _selectionNavigationController;
        private static LevelCollectionViewController? _collectionViewController;
        private static BeatmapSelectorViewController? _beatmapSelectorViewController;
        private ViewController? _viewController;

        #endregion

        #region Init & Dispose
        
        private IPreviewBeatmapLevel? _customPreviewBeatmapLevel;

        public void Setup(ViewController viewController) {
            _viewController = viewController;
        }
        
        protected override void OnInitialize() {
            _canvasGroup = _containerObject.AddComponent<CanvasGroup>();
            //_canvasGroup.ignoreParentGroups = true;
            _touchables = _tabSelectorObject.GetComponentsInChildren<Touchable>(true);
            _viewController = Content.GetComponentInParent<ViewController>();

            InitStaticDependencies();
            _collectionViewController!.didSelectLevelEvent += HandleSelectedBeatmapChanged;
            _beatmapSelectorViewController!.BeatmapSelectedEvent += HandleSelectorBeatmapSelected;
        }

        protected override void OnInstantiate() {
            _beatmapPreview = Instantiate<BeatmapPreviewCell>(transform);
            _beatmapPreview.PressedEvent += HandleBeatmapCellPressed;
        }

        protected override void OnDispose() {
            if (_collectionViewController) {
                _collectionViewController!.didSelectLevelEvent -= HandleSelectedBeatmapChanged;
            }
            if (_beatmapSelectorViewController) {
                _beatmapSelectorViewController!.BeatmapSelectedEvent -= HandleSelectorBeatmapSelected;
            }
        }

        private static void InitStaticDependencies() {
            var container = BeatSaberUI.DiContainer;
            if (!_selectionNavigationController) {
                _selectionNavigationController = container.Resolve<LevelSelectionNavigationController>();
            }
            if (!_beatmapSelectorViewController) {
                _beatmapSelectorViewController = _selectionNavigationController!.gameObject.AddComponent<BeatmapSelectorViewController>();
                _beatmapSelectorViewController.Init(_selectionNavigationController, container.Resolve<StandardLevelDetailViewController>());
            }
            if (!_collectionViewController) {
                _collectionViewController = container.Resolve<LevelCollectionViewController>();
            }
        }

        #endregion

        #region CurrentBeatmap

        private IPreviewBeatmapLevel? _currentPreviewBeatmapLevel;
        private bool _isFirstLaunch = true;
        private bool _currentTabOpened;
        private bool _isReady;
        private bool _currentBeatmapChanged;

        public void NotifyBeatmapSelectorReady(bool isReady) {
            if (isReady && _isFirstLaunch) {
                HandleSelectedBeatmapChanged(null, _selectionNavigationController!.selectedBeatmapLevel);
                _isFirstLaunch = false;
            }
            RefreshBeatmapPreview(_currentTabOpened && _currentBeatmapChanged);
            _currentBeatmapChanged = false;
            _isReady = isReady;
        }

        private void RefreshBeatmapPreview(bool notifyListeners = true) {
            _beatmapPreview.BlockPresses = _currentTabOpened;
            var level = _currentTabOpened ? _currentPreviewBeatmapLevel : _customPreviewBeatmapLevel;
            _beatmapPreview.SetData(level);
            if (notifyListeners) BeatmapSelectedEvent?.Invoke(level);
        }

        #endregion

        #region LevelSelection

        private bool _isSelectorOpened;

        public void ForceCloseLevelSelectionDialog() {
            CloseLevelSelectionDialog(true);
        }

        private void OpenLevelSelectionDialog() {
            if (_viewController is null) throw new UninitializedComponentException();
            _isSelectorOpened = true;
            _viewController.__PresentViewController(
                _beatmapSelectorViewController,null,
                ViewController.AnimationDirection.Vertical);
        }

        private void CloseLevelSelectionDialog(bool immediate = false) {
            RefreshBeatmapPreview();
            _isSelectorOpened = false;
            _beatmapSelectorViewController!.Dismiss(immediate);
        }

        #endregion

        #region SetActive

        public void SetActive(bool active) {
            _beatmapPreview.BlockPresses = _currentTabOpened || !active;
            _canvasGroup!.alpha = active ? 1f : 0.25f;
            foreach (var touchable in _touchables!) touchable.enabled = active;
        }

        #endregion

        #region Callbacks

        private void HandleSelectedBeatmapChanged(LevelCollectionViewController? controller, IPreviewBeatmapLevel level) {
            if (_isReady || _isSelectorOpened) return;
            _currentPreviewBeatmapLevel = level;
            _currentBeatmapChanged = true;
        }

        private void HandleSelectorBeatmapSelected(IDifficultyBeatmap beatmap) {
            _customPreviewBeatmapLevel = beatmap.level;
            RefreshBeatmapPreview();
        }

        private void HandleBeatmapCellPressed() {
            OpenLevelSelectionDialog();
        }

        [UIAction("select-tab"), UsedImplicitly]
        private void HandleTabSelected(SegmentedControl segmentedControl, int idx) {
            var tabName = segmentedControl.cells[idx].GetComponentInChildren<TMP_Text>().text;
            _currentTabOpened = tabName is "current";
            RefreshBeatmapPreview();
        }

        #endregion
    }

}