using System;
using BeatLeader.Components;
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
        private static bool LevelCollectionTablePrefix(IPreviewBeatmapLevel? beatmapLevel) {
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

        public event Action<IDifficultyBeatmap>? BeatmapSelectedEvent;

        #endregion

        #region LevelNavigationController

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
            //
            _patchedClickedEvent = new();
            _patchedClickedEvent.AddListener(HandleActionButtonPressed);
        }

        #endregion

        #region LevelNavigationController

        private SelectLevelCategoryViewController.LevelCategory _lastSelectedLevelCategory = SelectLevelCategoryViewController.LevelCategory.All;
        private IBeatmapLevelPack? _lastSelectedBeatmapLevelPack;

        private SelectLevelCategoryViewController.LevelCategory _originalLevelCategory;
        private IPreviewBeatmapLevel? _originalPreviewBeatmapLevel;
        private IBeatmapLevelPack? _originalBeatmapLevelPack;
        private BeatmapDifficultyMask _originalAllowedBeatmapDifficultyMask;
        private BeatmapCharacteristicSO[] _originalNotAllowedCharacteristics = Array.Empty<BeatmapCharacteristicSO>();
        private bool _originalHidePacksIfOneOrNone;
        private bool _originalHidePracticeButton;
        private string _originalActionButtonText = string.Empty;

        private void PatchLevelNavigationController() {
            //saving initial values
            _originalLevelCategory = _levelSelectionNavigationController.selectedLevelCategory;
            _originalPreviewBeatmapLevel = _levelCollectionNavigationController.selectedBeatmapLevel;
            _originalBeatmapLevelPack = _levelSelectionNavigationController.selectedBeatmapLevelPack;
            //TODO: asm pub
            _originalAllowedBeatmapDifficultyMask = _levelSelectionNavigationController.GetField<BeatmapDifficultyMask, LevelSelectionNavigationController>("_allowedBeatmapDifficultyMask");
            _originalNotAllowedCharacteristics = _levelSelectionNavigationController.GetField<BeatmapCharacteristicSO[], LevelSelectionNavigationController>("_notAllowedCharacteristics");
            _originalHidePacksIfOneOrNone = _levelSelectionNavigationController.GetField<bool, LevelSelectionNavigationController>("_hidePacksIfOneOrNone");
            _originalHidePracticeButton = _levelSelectionNavigationController.GetField<bool, LevelSelectionNavigationController>("_hidePracticeButton");
            _originalActionButtonText = _levelSelectionNavigationController.GetField<string, LevelSelectionNavigationController>("_actionButtonText");
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
                _originalPreviewBeatmapLevel,
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
                _originalPreviewBeatmapLevel,
                true
            );
            NavigateToBeatmap(_originalPreviewBeatmapLevel, _originalLevelCategory);
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

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            _levelDetailViewController.didChangeContentEvent += HandleContentChanged;
            _lastClickedEvent = _levelDetailView.actionButton.onClick;
            _levelDetailView.actionButton.onClick = _patchedClickedEvent;
            //
            PatchLevelDetail();
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            PatchLevelNavigationController();
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
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

        private void NavigateToBeatmap(IPreviewBeatmapLevel? level, SelectLevelCategoryViewController.LevelCategory category) {
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
            BeatmapSelectedEvent?.Invoke(_levelCollectionNavigationController.selectedDifficultyBeatmap);
        }

        #endregion
    }
}