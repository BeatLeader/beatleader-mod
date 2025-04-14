using System;
using System.Collections.Generic;
using BeatSaberMarkupLanguage.Attributes;
using HMUI;
using IPA.Utilities;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace BeatLeader.Components {
    internal class BeatmapSelector : ReeUIComponentV2 {
        #region Events

        public event Action<BeatmapLevel?>? BeatmapSelectedEvent;

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

        private LevelSelectionNavigationController? _selectionNavigationController;
        private LevelCollectionViewController? _collectionViewController;
        private BeatmapSelectorViewController? _beatmapSelectorViewController;
        private FlowCoordinator? _flowCoordinator;
        private ViewController? _viewController;

        #endregion

        #region Init & Dispose

        private bool IsInitialized => _isInitializedComponent && _isInitializedDependencies;

        private BeatmapLevel? _customPreviewBeatmapLevel;

        private bool _isInitializedComponent;
        private bool _isInitializedDependencies;
        private bool _isActive;

        public void Setup(
            ViewController viewController,
            FlowCoordinator flowCoordinator,
            LevelSelectionNavigationController levelSelectionNavigationController,
            LevelCollectionViewController levelCollectionViewController,
            StandardLevelDetailViewController standardLevelDetailViewController
        ) {
            _viewController = viewController;
            _flowCoordinator = flowCoordinator;
            _selectionNavigationController = levelSelectionNavigationController;
            _collectionViewController = levelCollectionViewController;

            _beatmapSelectorViewController = levelSelectionNavigationController.gameObject.AddComponent<BeatmapSelectorViewController>();
            _beatmapSelectorViewController.Init(levelSelectionNavigationController, standardLevelDetailViewController);
            _beatmapSelectorViewController.BeatmapSelectedEvent += HandleSelectorBeatmapSelected;
            _beatmapSelectorViewController.didDeactivateEvent += HandleSelectorDidDeactivate;
            _beatmapSelectorViewController.didActivateEvent += HandleSelectorDidActivate;

            _collectionViewController.didSelectLevelEvent += HandleSelectedBeatmapChanged;
            _isInitializedDependencies = true;
        }

        protected override void OnInitialize() {
            _canvasGroup = _containerObject.AddComponent<CanvasGroup>();
            _canvasGroup.ignoreParentGroups = true;
            _touchables = _tabSelectorObject.GetComponentsInChildren<Touchable>(true);
            _isInitializedComponent = true;
        }

        protected override void OnInstantiate() {
            _beatmapPreview = Instantiate<BeatmapPreviewCell>(transform);
            _beatmapPreview.PressedEvent += HandleBeatmapCellPressed;
        }

        protected override void OnDispose() {
            if (_collectionViewController != null) {
                _collectionViewController.didSelectLevelEvent -= HandleSelectedBeatmapChanged;
            }
            if (_beatmapSelectorViewController != null) {
                _beatmapSelectorViewController.BeatmapSelectedEvent -= HandleSelectorBeatmapSelected;
                _beatmapSelectorViewController.didDeactivateEvent -= HandleSelectorDidDeactivate;
                _beatmapSelectorViewController.didActivateEvent -= HandleSelectorDidActivate;
                Destroy(_beatmapSelectorViewController);
            }
        }

        #endregion

        #region CurrentBeatmap

        private BeatmapLevel? _currentPreviewBeatmapLevel;
        private bool _isFirstLaunch = true;
        private bool _currentTabOpened;
        private bool _isReady;
        private bool _currentBeatmapChanged;

        public void NotifyBeatmapSelectorReady(bool isReady) {
            if (!IsInitialized) throw new UninitializedComponentException();
            if (isReady && _isFirstLaunch) {
                HandleSelectedBeatmapChanged(null, _selectionNavigationController!.beatmapLevel);
                _isFirstLaunch = false;
            }
            RefreshBeatmapPreview(_currentTabOpened && _currentBeatmapChanged);
            _currentBeatmapChanged = false;
            _isReady = isReady;
        }

        private void RefreshBeatmapPreview(bool notifyListeners = true) {
            _beatmapPreview.BlockPresses = _currentTabOpened || !_isActive;
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
            if (!IsInitialized) throw new UninitializedComponentException();
            //SetFlowCoordinatorBackButtonEnabled(false);
            _viewController!.__PresentViewController(
                _beatmapSelectorViewController,
                null,
                ViewController.AnimationDirection.Vertical
            );
        }

        private void CloseLevelSelectionDialog(bool immediate = false) {
            if (!IsInitialized) throw new UninitializedComponentException();
            RefreshBeatmapPreview();
            _beatmapSelectorViewController!.Dismiss(immediate);
            //SetFlowCoordinatorBackButtonEnabled(true);
        }

        private void HandleSelectorDidActivate(
            bool firstActivation,
            bool addedToHierarchy,
            bool screenSystemEnabling
        ) {
            _isSelectorOpened = true;
        }

        private void HandleSelectorDidDeactivate(
            bool removedFromHierarchy,
            bool screenSystemDisabling
        ) {
            _isSelectorOpened = false;
        }

        private void SetFlowCoordinatorBackButtonEnabled(bool buttonEnabled) {
            _flowCoordinator!.GetField<ScreenSystem, FlowCoordinator>(
                "_screenSystem"
            ).SetBackButton(buttonEnabled, true);
        }

        #endregion

        #region SetActive

        public void SetActive(bool active) {
            if (!_isInitializedComponent) return;
            _isActive = active;
            _beatmapPreview.BlockPresses = _currentTabOpened || !active;
            _canvasGroup!.alpha = active ? 1f : 0.25f;
            foreach (var touchable in _touchables!) touchable.enabled = active;
        }

        #endregion

        #region Callbacks

        private void HandleSelectedBeatmapChanged(LevelCollectionViewController? controller, BeatmapLevel level) {
            if (_isSelectorOpened) {
                return;
            }
            _currentPreviewBeatmapLevel = level;
            _currentBeatmapChanged = true;
        }

        private void HandleSelectorBeatmapSelected(BeatmapLevel level) {
            _customPreviewBeatmapLevel = level;
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