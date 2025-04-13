using System;
using System.Collections.Generic;
using BGLib.Polyglot;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace BeatLeader.Components {
    internal class BeatmapSelectorViewController : DummyViewController {
        #region Events

        public event Action<BeatmapLevel>? BeatmapSelectedEvent;

        #endregion

        #region UI Components

        private LevelDetailWrapper? _levelDetailWrapper;
        private CloseButton? _closeButton;

        private LevelSelectionNavigationController _levelSelectionNavigationController = null!;
        private LevelCollectionNavigationController _levelCollectionNavigationController = null!;
        private LevelFilteringNavigationController _levelFilteringNavigationController = null!;
        private StandardLevelDetailViewController _levelDetailViewController = null!;
        private IconSegmentedControl _levelCategorySegmentedControl = null!;
        private Transform _levelDetailLevelBar = null!;
        private Transform _levelDetail = null!;

        #endregion

        #region Init & Dispose

        private Vector3 _levelDetailLevelBarOriginalPos;
        private SelectLevelCategoryViewController.LevelCategory _lastSelectedLevelCategory = SelectLevelCategoryViewController.LevelCategory.All;
        private BeatmapLevel? _originalBeatmapLevel;
        private bool _isInitialized;

        private string _defaultPlayButtonText = Localization.Get("BUTTON_PLAY");
        private static string CUSTOM_PLAY_BUTTON = "SELECT";

        public void Init(
            LevelSelectionNavigationController levelSelectionNavigationController,
            StandardLevelDetailViewController levelDetailViewController
        ) {
            base.Init(levelSelectionNavigationController);
            _levelSelectionNavigationController = levelSelectionNavigationController;
            _levelCollectionNavigationController = levelSelectionNavigationController.GetField
                <LevelCollectionNavigationController, LevelSelectionNavigationController>("_levelCollectionNavigationController");
            _levelFilteringNavigationController = _levelSelectionNavigationController.GetField
                <LevelFilteringNavigationController, LevelSelectionNavigationController>("_levelFilteringNavigationController");
            var selectLevelCategoryViewController = _levelFilteringNavigationController.GetField
                <SelectLevelCategoryViewController, LevelFilteringNavigationController>("_selectLevelCategoryViewController");
            _levelFilteringNavigationController.SetupBeatmapLevelPacks();
            var levelCategories = _levelFilteringNavigationController.GetField
                <SelectLevelCategoryViewController.LevelCategory[], LevelFilteringNavigationController>("_enabledLevelCategories");
            selectLevelCategoryViewController.Setup(levelCategories[0], levelCategories);
            _levelCategorySegmentedControl = selectLevelCategoryViewController.GetComponentInChildren<IconSegmentedControl>(true);
            _levelDetailViewController = levelDetailViewController;
            _levelDetail = _levelDetailViewController.transform.Find("LevelDetail");
            _levelDetailLevelBar = _levelDetail.Find("LevelBarBig");
            _isInitialized = true;
        }

        public override void OnDestroy() {
            base.OnDestroy();
            if (_levelDetailWrapper) Destroy(_levelDetailWrapper!.gameObject);
            if (_closeButton) Destroy(_closeButton!.gameObject);
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (!_isInitialized) throw new UninitializedComponentException();
            _levelSelectionNavigationController.Setup(
                SongPackMask.all,
                BeatmapDifficultyMask.All,
                Array.Empty<BeatmapCharacteristicSO>(),
                false,
                false,
                CUSTOM_PLAY_BUTTON,
                null,
                SelectLevelCategoryViewController.LevelCategory.CustomSongs,
                null,
                true
            );
            if (firstActivation) {
                _levelDetailWrapper = ReeUIComponentV2.Instantiate<LevelDetailWrapper>(_levelDetail.parent);
                _levelDetailWrapper.SelectButtonPressedEvent += HandleSelectButtonPressed;
                _closeButton = ReeUIComponentV2.Instantiate<CloseButton>(_levelFilteringNavigationController.transform);
                _closeButton.ManualInit(_closeButton.transform);
                _closeButton.ButtonPressedEvent += HandleCloseButtonPressed;
            }

            _levelSelectionNavigationController._actionButtonText = CUSTOM_PLAY_BUTTON;
            _originalBeatmapLevel = _levelCollectionNavigationController.beatmapLevel;

            SetLevelDetailWrapperEnabled(true);
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            NavigateToBeatmap(null, _lastSelectedLevelCategory);
            SetCloseButtonEnabled(true);
            _levelCollectionNavigationController.HideDetailViewController();
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            _levelSelectionNavigationController._actionButtonText = _defaultPlayButtonText;
            SetLevelDetailWrapperEnabled(false);
            SetCloseButtonEnabled(false);

            if (_originalBeatmapLevel != null) {
                SetSelectedLevel(_originalBeatmapLevel);
            }

            _lastSelectedLevelCategory = _levelSelectionNavigationController.selectedLevelCategory;
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }

        #endregion

        #region Navigation

        public void Dismiss(bool immediate = false) {
            if (!_isActivated) return;
            __DismissViewController(null, AnimationDirection.Vertical, immediate);
        }

        private void NavigateToBeatmap(BeatmapLevel? level, SelectLevelCategoryViewController.LevelCategory category) {
            if (category is SelectLevelCategoryViewController.LevelCategory.None) return;
            var cell = _levelCategorySegmentedControl.cells[(int)category - 1];
            cell.SetSelected(true, SelectableCell.TransitionType.Animated, cell, false);
            if (level is null) return;
            _levelCollectionNavigationController.SelectLevel(level);
            _levelCollectionNavigationController.HandleLevelCollectionViewControllerDidSelectLevel(null, level);
        }

        private void SetSelectedLevel(BeatmapLevel level) {
            var controller = _levelCollectionNavigationController;
            var detail = _levelDetailViewController;

            _levelCollectionNavigationController._beatmapLevelToBeSelectedAfterPresent = level;
            _levelCollectionNavigationController._levelCollectionViewController._beatmapLevelToBeSelected = level;

            detail._canBuyPack = controller._levelPack != null;
            detail._pack = controller._levelPack ?? detail._beatmapLevelsModel.GetLevelPackForLevelId(level.levelID);
            detail._standardLevelDetailView.hidePracticeButton = !controller._showPracticeButtonInDetailView;
            detail._standardLevelDetailView.actionButtonText = controller._actionButtonTextInDetailView;
            detail._allowedBeatmapDifficultyMask = controller._allowedBeatmapDifficultyMask;
            detail._notAllowedCharacteristics = new HashSet<BeatmapCharacteristicSO>(controller._notAllowedCharacteristics);
            detail._notAllowedCharacteristics.UnionWith(detail._beatmapCharacteristicCollection.disabledBeatmapCharacteristics);
            detail._contentIsOwnedAndReady = false;
        }

        #endregion

        #region UI Tweaks

        private void SetCloseButtonEnabled(bool buttonEnabled) {
            _closeButton!.GetRootTransform().localPosition = new(-69, 0, 0);
            _closeButton.SetRootActive(buttonEnabled);
        }

        private void SetLevelDetailWrapperEnabled(bool wrapperEnabled) {
            if (wrapperEnabled) {
                _levelDetailViewController.didChangeContentEvent += HandleContentChanged;
                _levelDetailLevelBarOriginalPos = _levelDetailLevelBar.localPosition;
                _levelDetailWrapper!.Setup(_levelDetailLevelBar);
                _levelDetailWrapper.Size = new(70, 56);
            } else {
                _levelDetailViewController.didChangeContentEvent -= HandleContentChanged;
                _levelDetailLevelBar.SetParent(_levelDetail, false);
                _levelDetailWrapper!.gameObject.SetActive(false);
                _levelDetailLevelBar.localPosition = _levelDetailLevelBarOriginalPos;
            }
        }

        #endregion

        #region Callbacks

        private void HandleContentChanged(
            StandardLevelDetailViewController controller,
            StandardLevelDetailViewController.ContentType contentType
        ) {
            if (contentType is not StandardLevelDetailViewController.ContentType.OwnedAndReady) {
                _levelDetailWrapper!.gameObject.SetActive(false);
                return;
            }
            _levelDetailWrapper!.gameObject.SetActive(true);
            _levelDetail.gameObject.SetActive(false);
        }

        private void HandleCloseButtonPressed() {
            Dismiss();
        }

        private void HandleSelectButtonPressed() {
            BeatmapSelectedEvent?.Invoke(_levelSelectionNavigationController.beatmapLevel);
            HandleCloseButtonPressed();
        }

        #endregion
    }
}