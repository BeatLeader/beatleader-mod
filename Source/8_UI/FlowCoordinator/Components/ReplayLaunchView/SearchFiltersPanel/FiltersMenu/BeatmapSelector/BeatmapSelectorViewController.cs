using System;
using System.Linq;
using System.Reflection;
using HMUI;
using IPA.Utilities;
using Polyglot;
using UnityEngine;

namespace BeatLeader.Components {
    internal class BeatmapSelectorViewController : DummyViewController {
        #region Events

        public event Action<IPreviewBeatmapLevel>? BeatmapSelectedEvent;

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

        private const string SelectButtonText = "SELECT";

        private Vector3 _levelDetailLevelBarOriginalPos;
        private SelectLevelCategoryViewController.LevelCategory _originalLevelCategory = SelectLevelCategoryViewController.LevelCategory.All;
        private SelectLevelCategoryViewController.LevelCategory _lastSelectedLevelCategory = SelectLevelCategoryViewController.LevelCategory.All;
        private IPreviewBeatmapLevel? _originalPreviewBeatmapLevel;
        private bool _isInitialized;
        private bool _wasActivatedWithCoordinator;

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

        protected override void OnDestroy() {
            base.OnDestroy();
            if (_levelDetailWrapper) Destroy(_levelDetailWrapper!.gameObject);
            if (_closeButton) Destroy(_closeButton!.gameObject);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (!_isInitialized) throw new UninitializedComponentException();
            if (firstActivation) {
                //super tricky-wicked way to check is controller was ever presented with solo flow coordinator
                _wasActivatedWithCoordinator = _levelSelectionNavigationController
                    .GetField<string, LevelSelectionNavigationController>("_actionButtonText") == Localization.Get("BUTTON_PLAY");
                _levelSelectionNavigationController.Setup(
                    SongPackMask.all,
                    BeatmapDifficultyMask.All,
                    Array.Empty<BeatmapCharacteristicSO>(),
                    false,
                    false,
                    SelectButtonText,
                    null,
                    SelectLevelCategoryViewController.LevelCategory.None,
                    null,
                    true);
                _levelDetailWrapper = ReeUIComponentV2.Instantiate<LevelDetailWrapper>(_levelDetail.parent);
                _levelDetailWrapper.SelectButtonPressedEvent += HandleSelectButtonPressed;
                _closeButton = ReeUIComponentV2.Instantiate<CloseButton>(_levelFilteringNavigationController.transform);
                _closeButton.ButtonPressedEvent += HandleCloseButtonPressed;
            }
            _originalLevelCategory = _levelSelectionNavigationController.selectedLevelCategory;
            _originalPreviewBeatmapLevel = _levelCollectionNavigationController.selectedBeatmapLevel;
            SetLevelDetailWrapperEnabled(true);
            SetCloseButtonEnabled(true);
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            NavigateToBeatmap(null, _lastSelectedLevelCategory);
            _levelCollectionNavigationController.HideDetailViewController();
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            SetLevelDetailWrapperEnabled(false);
            SetCloseButtonEnabled(false);
            _lastSelectedLevelCategory = _levelSelectionNavigationController.selectedLevelCategory;
            NavigateToBeatmap(_originalPreviewBeatmapLevel, _originalLevelCategory);
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }

        #endregion

        #region Navigation

        public void Dismiss(bool immediate = false) {
            if (!_isActivated) return;
            __DismissViewController(null, AnimationDirection.Vertical, immediate);
        }

        private void NavigateToBeatmap(IPreviewBeatmapLevel? level, SelectLevelCategoryViewController.LevelCategory category) {
            if (category is SelectLevelCategoryViewController.LevelCategory.None) return;
            var cell = _levelCategorySegmentedControl.cells[(int)category - 1];
            cell.SetSelected(true, SelectableCell.TransitionType.Animated, cell, false);
            if (level is null) return;
            _levelCollectionNavigationController.SelectLevel(level);
            _levelCollectionNavigationController.HandleLevelCollectionViewControllerDidSelectLevel(null, level);
        }

        #endregion

        #region UI Tweaks

        private void SetCloseButtonEnabled(bool buttonEnabled) {
            var buttonTransform = _closeButton!.transform;
            buttonTransform.localPosition = new(_wasActivatedWithCoordinator ? -95 : -80, 5, 0);
            _closeButton.gameObject.SetActive(buttonEnabled);
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
            BeatmapSelectedEvent?.Invoke(_levelSelectionNavigationController.selectedBeatmapLevel);
            HandleCloseButtonPressed();
        }

        #endregion
    }
}