using System;
using BeatLeader.Components;
using BeatLeader.Models;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using Reactive;
using UnityEngine.UI;
using Zenject;

namespace BeatLeader.UI.Hub {
    [HarmonyPatch]
    internal class LevelSelectionViewController : DummyViewController {
        #region Patch

        [HarmonyPatch(typeof(LevelCollectionTableView), "SelectLevel"), HarmonyPrefix]
        private static bool LevelCollectionTablePrefix(BeatmapLevel? beatmapLevel) {
            return beatmapLevel != null;
        }

        #endregion

        #region Injection

        [Inject] private readonly LevelSelectionNavigationController _levelSelectionNavigationController = null!;
        [Inject] private readonly LevelCollectionNavigationController _levelCollectionNavigationController = null!;
        [Inject] private readonly LevelFilteringNavigationController _levelFilteringNavigationController = null!;
        [Inject] private readonly SelectLevelCategoryViewController _levelCategoryViewController = null!;
        [Inject] private readonly StandardLevelDetailViewController _levelDetailViewController = null!;

        #endregion

        #region Events

        public event Action<BeatmapLevelWithKey>? BeatmapSelectedEvent;

        #endregion

        #region LevelNavigationController

        private IconSegmentedControl _levelCategorySegmentedControl = null!;
        private StandardLevelDetailView _levelDetailView = null!;

        private void Awake() {
            //init
            Init(_levelSelectionNavigationController);
            _levelFilteringNavigationController.SetupBeatmapLevelPacks();
            var levelCategories = _levelFilteringNavigationController._enabledLevelCategories;
            //
            _levelCategoryViewController.Setup(levelCategories[0], levelCategories);
            _levelCategorySegmentedControl = _levelCategoryViewController.GetComponentInChildren<IconSegmentedControl>(true);
            _levelDetailView = _levelDetailViewController._standardLevelDetailView;
            //construct selection detail view
            _selectionDetailView = new() { Enabled = false };
            _selectionDetailView.WithRectExpand().Use(_levelDetailViewController.transform);
            _selectionDetailView.Setup(_levelDetailView);
            //
            _patchedClickedEvent = new();
            _patchedClickedEvent.AddListener(HandleActionButtonPressed);
        }

        #endregion

        #region LevelNavigationController

        private SelectLevelCategoryViewController.LevelCategory _lastSelectedLevelCategory = SelectLevelCategoryViewController.LevelCategory.All;
        private BeatmapLevelPack? _lastSelectedBeatmapLevelPack;

        private SelectLevelCategoryViewController.LevelCategory _originalLevelCategory;
        private BeatmapLevel? _originalBeatmapLevel;
        private BeatmapLevelPack? _originalBeatmapLevelPack;
        private BeatmapDifficultyMask _originalAllowedBeatmapDifficultyMask;
        private bool _originalHidePacksIfOneOrNone;
        private bool _originalHidePracticeButton;
        private string _originalActionButtonText = string.Empty;

        private void PatchLevelNavigationController() {
            //saving initial values
            _originalLevelCategory = _levelSelectionNavigationController.selectedLevelCategory;
            _originalBeatmapLevel = _levelCollectionNavigationController.beatmapLevel;
            _originalBeatmapLevelPack = _levelSelectionNavigationController.selectedBeatmapLevelPack;
            _originalAllowedBeatmapDifficultyMask = _levelSelectionNavigationController._allowedBeatmapDifficultyMask;
            _originalHidePacksIfOneOrNone = _levelSelectionNavigationController._hidePacksIfOneOrNone;
            _originalHidePracticeButton = _levelSelectionNavigationController._hidePracticeButton;
            _originalActionButtonText = _levelSelectionNavigationController._actionButtonText;
            //presenting
            _levelSelectionNavigationController.ClearSelected();
            _levelSelectionNavigationController.Setup(
                SongPackMask.all,
                BeatmapDifficultyMask.All,
                Array.Empty<BeatmapCharacteristicSO>(),
                false,
                true,
                "SELECT",
                _lastSelectedBeatmapLevelPack,
                _lastSelectedLevelCategory,
                _originalBeatmapLevel,
                true
            );
            NavigateToBeatmap(null, _lastSelectedLevelCategory);
        }

        private void UnpatchLevelNavigationController() {
            //restoring initial values
            _levelSelectionNavigationController.ClearSelected();
            _levelSelectionNavigationController.Setup(
                SongPackMask.all,
                _originalAllowedBeatmapDifficultyMask,
                Array.Empty<BeatmapCharacteristicSO>(),
                _originalHidePacksIfOneOrNone,
                _originalHidePracticeButton,
                _originalActionButtonText,
                _originalBeatmapLevelPack,
                _originalLevelCategory,
                _originalBeatmapLevel,
                true
            );
            NavigateToBeatmap(_originalBeatmapLevel, _originalLevelCategory);
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

        private Button.ButtonClickedEvent? _lastClickedEvent;
        private Button.ButtonClickedEvent? _patchedClickedEvent;

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            _levelDetailViewController.didChangeContentEvent += HandleContentChanged;
            _lastClickedEvent = _levelDetailView.actionButton.onClick;
            _levelDetailView.actionButton.onClick = _patchedClickedEvent;
            //
            PatchLevelDetail();
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            PatchLevelNavigationController();
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            _levelDetailViewController.didChangeContentEvent -= HandleContentChanged;
            _levelDetailView.actionButton.onClick = _lastClickedEvent;
            //
            _lastSelectedLevelCategory = _levelSelectionNavigationController.selectedLevelCategory;
            _lastSelectedBeatmapLevelPack = _levelSelectionNavigationController.selectedBeatmapLevelPack;
            UnpatchLevelNavigationController();
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
        }

        public override void DeactivateGameObject() {
            UnpatchLevelDetail();
            base.DeactivateGameObject();
        }

        #endregion

        #region Navigation

        public void Dismiss(bool immediate = false) {
            if (!_isActivated) return;
            __DismissViewController(null, AnimationDirection.Vertical, immediate);
        }

        private void NavigateToBeatmap(BeatmapLevel? level, SelectLevelCategoryViewController.LevelCategory category) {
            if (category is SelectLevelCategoryViewController.LevelCategory.None) return;
            //
            var cell = _levelCategorySegmentedControl.cells[(int)category - 1];
            cell.SetSelected(true, SelectableCell.TransitionType.Instant, cell, true);
            //
            if (level == null) {
                _levelCollectionNavigationController.HideDetailViewController();
                return;
            }
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

        private void HandleActionButtonPressed() {
            var level = new BeatmapLevelWithKey(
                _levelCollectionNavigationController.beatmapLevel,
                _levelCollectionNavigationController.beatmapKey
            );
            BeatmapSelectedEvent?.Invoke(level);
        }

        #endregion
    }
}