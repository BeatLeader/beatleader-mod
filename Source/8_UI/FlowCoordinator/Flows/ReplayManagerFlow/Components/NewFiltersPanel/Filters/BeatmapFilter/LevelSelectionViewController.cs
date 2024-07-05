using System;
using BeatLeader.Components;
using BeatLeader.UI.Reactive;
using HMUI;
using IPA.Utilities;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class LevelSelectionViewController : DummyViewController {
        #region Injection

        [Inject] private readonly LevelSelectionNavigationController _levelSelectionNavigationController = null!;
        [Inject] private readonly LevelCollectionNavigationController _levelCollectionNavigationController = null!;
        [Inject] private readonly LevelFilteringNavigationController _levelFilteringNavigationController = null!;
        [Inject] private readonly SelectLevelCategoryViewController _levelCategoryViewController = null!;
        [Inject] private readonly StandardLevelDetailViewController _levelDetailViewController = null!;

        #endregion

        #region Events

        public event Action<IDifficultyBeatmap>? BeatmapSelectedEvent;

        #endregion

        #region LevelNavigationController

        private SelectLevelCategoryViewController.LevelCategory _originalLevelCategory = SelectLevelCategoryViewController.LevelCategory.All;
        private SelectLevelCategoryViewController.LevelCategory _lastSelectedLevelCategory = SelectLevelCategoryViewController.LevelCategory.All;
        private IPreviewBeatmapLevel? _originalPreviewBeatmapLevel;
        
        private IconSegmentedControl _levelCategorySegmentedControl = null!;
        private StandardLevelDetailView _levelDetailView = null!;

        private void Awake() {
            //init
            Init(_levelSelectionNavigationController);
            //TODO: asm pub
            _levelFilteringNavigationController.SetupBeatmapLevelPacks();
            var levelCategories = _levelFilteringNavigationController.GetField<
                SelectLevelCategoryViewController.LevelCategory[], LevelFilteringNavigationController>("_enabledLevelCategories");
            //
            _levelCategoryViewController.Setup(levelCategories[0], levelCategories);
            _levelCategorySegmentedControl = _levelCategoryViewController.GetComponentInChildren<IconSegmentedControl>(true);
            _levelDetailView = _levelDetailViewController.GetField<StandardLevelDetailView, StandardLevelDetailViewController>("_standardLevelDetailView"); 
            //construct selection detail view
            _selectionDetailView = new() { Enabled = false };
            _selectionDetailView.WithRectExpand().Use(_levelDetailViewController.transform);
            _selectionDetailView.Setup(_levelDetailView);
        }

        private void SetupLevelNavigationController() {
            _levelSelectionNavigationController.Setup(
                SongPackMask.all,
                BeatmapDifficultyMask.All,
                Array.Empty<BeatmapCharacteristicSO>(),
                false,
                true,
                "SELECT",
                null,
                SelectLevelCategoryViewController.LevelCategory.None,
                null,
                true
            );
        }

        #endregion

        #region LevelDetail

        public bool allowDifficultySelection = true;

        private LevelSelectionDetailView _selectionDetailView = null!;
        
        private void PatchLevelDetail() {
            if (allowDifficultySelection) return;
            _levelDetailView.gameObject.SetActive(false);
            _selectionDetailView.Enabled = true;
            _selectionDetailView.Refresh();
        }

        private void UnpatchLevelDetail() {
            if (allowDifficultySelection) return;
            _levelDetailView.gameObject.SetActive(true);
            _selectionDetailView.Enabled = false;
        }

        #endregion

        #region Setup

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            _levelCollectionNavigationController.didPressActionButtonEvent += HandleActionButtonPressed;
            _levelDetailViewController.didChangeContentEvent += HandleContentChanged;
            //
            SetupLevelNavigationController();
            _originalLevelCategory = _levelSelectionNavigationController.selectedLevelCategory;
            _originalPreviewBeatmapLevel = _levelCollectionNavigationController.selectedBeatmapLevel;
            NavigateToBeatmap(null, _lastSelectedLevelCategory);
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            //
            _levelCollectionNavigationController.HideDetailViewController();
            PatchLevelDetail();
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            _levelCollectionNavigationController.didPressActionButtonEvent -= HandleActionButtonPressed;
            _levelDetailViewController.didChangeContentEvent -= HandleContentChanged;
            //
            _lastSelectedLevelCategory = _levelSelectionNavigationController.selectedLevelCategory;
            NavigateToBeatmap(_originalPreviewBeatmapLevel, _originalLevelCategory);
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            UnpatchLevelDetail();
        }

        #endregion

        #region Navigation

        public void Dismiss(bool immediate = false) {
            if (!_isActivated) return;
            __DismissViewController(null, AnimationDirection.Vertical, immediate);
        }

        private void NavigateToBeatmap(IPreviewBeatmapLevel? level, SelectLevelCategoryViewController.LevelCategory category) {
            if (category is SelectLevelCategoryViewController.LevelCategory.None) return;
            //
            var cell = _levelCategorySegmentedControl.cells[(int)category - 1];
            cell.SetSelected(true, SelectableCell.TransitionType.Instant, cell, false);
            //
            if (level is null) return;
            _levelCollectionNavigationController.SelectLevel(level);
            _levelCollectionNavigationController.HandleLevelCollectionViewControllerDidSelectLevel(null, level);
        }

        #endregion

        #region Callbacks

        private void HandleContentChanged(StandardLevelDetailViewController _, StandardLevelDetailViewController.ContentType contentType) {
            if (allowDifficultySelection) return;
            //
            if (contentType is not StandardLevelDetailViewController.ContentType.OwnedAndReady) {
                _selectionDetailView.Enabled = false;
                return;
            }
            _selectionDetailView.Enabled = true;
            _levelDetailView.gameObject.SetActive(false);
            _selectionDetailView.Refresh();
        }
        
        private void HandleActionButtonPressed(LevelCollectionNavigationController controller) {
            BeatmapSelectedEvent?.Invoke(controller.selectedDifficultyBeatmap);
        }

        #endregion
    }
}