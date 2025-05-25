using System;
using BeatLeader.Components;
using BeatLeader.Models;
using HarmonyLib;
using HMUI;
using IPA.Utilities;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Yoga;
using UnityEngine;
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

            _levelCategoryViewController.Setup(levelCategories[0], levelCategories);
            _levelCategorySegmentedControl = _levelCategoryViewController.GetComponentInChildren<IconSegmentedControl>(true);
            _levelDetailView = _levelDetailViewController._standardLevelDetailView;

            var levelDetailTransform = _levelDetailView.transform;
            _detailViewButtons = levelDetailTransform.Find("ActionButtons").gameObject;

            // Custom button to avoid messing with base game callbacks
            new Layout {
                    ContentTransform = {
                        pivot = new Vector2(0.5f, 0f)
                    },
                    Children = {
                        new BsPrimaryButton {
                            OnClick = HandleActionButtonPressed,
                            Text = "SELECT"
                        }.AsFlexItem(size: new() { x = 24f })
                    }
                }
                .AsFlexItem(size: new() { y = 10f })
                .AsFlexGroup(
                    justifyContent: Justify.Center,
                    alignItems: Align.Stretch,
                    padding: 1f
                )
                .Bind(ref _detailViewCustomButtons)
                .Use(levelDetailTransform);

            // Custom selector for cases when characteristic and difficulty selectors are not needed 
            new LevelSelectionDetailView {
                    Enabled = false,
                    OnClick = HandleActionButtonPressed
                }
                .Bind(ref _selectionDetailView)
                .WithRectExpand()
                .Use(_levelDetailViewController.transform);

            _selectionDetailView.Setup(_levelDetailView);
        }

        #endregion

        #region LevelNavigationController

        private SelectLevelCategoryViewController.LevelCategory _lastSelectedLevelCategory = SelectLevelCategoryViewController.LevelCategory.None;
        private BeatmapLevelPack? _lastSelectedBeatmapLevelPack;

        private SelectLevelCategoryViewController.LevelCategory _originalLevelCategory;
        private BeatmapLevelPack? _originalBeatmapLevelPack;
        private BeatmapLevel? _originalBeatmapLevel;
        private BeatmapDifficultyMask _originalAllowedBeatmapDifficultyMask;
        private bool _originalHidePacksIfOneOrNone;

        private void PatchLevelNavigationController() {
            //saving initial values
            _originalLevelCategory = _levelSelectionNavigationController.selectedLevelCategory;
            _originalBeatmapLevel = _levelCollectionNavigationController.beatmapLevel;
            _originalAllowedBeatmapDifficultyMask = _levelSelectionNavigationController._allowedBeatmapDifficultyMask;
            _originalHidePacksIfOneOrNone = _levelSelectionNavigationController._hidePacksIfOneOrNone;
            _originalBeatmapLevelPack = _levelSelectionNavigationController.selectedBeatmapLevelPack;
            //presenting
            _levelCollectionNavigationController._levelCollectionViewController._levelCollectionTableView.ClearSelection();
            _levelSelectionNavigationController._notAllowedCharacteristics = Array.Empty<BeatmapCharacteristicSO>();
            _levelSelectionNavigationController._allowedBeatmapDifficultyMask = BeatmapDifficultyMask.All;

            _levelFilteringNavigationController.Setup(
                SongPackMask.all,
                _lastSelectedBeatmapLevelPack,
                _lastSelectedLevelCategory,
                false,
                true
            );
        }

        private void UnpatchLevelNavigationController() {
            //restoring initial values
            _levelSelectionNavigationController._allowedBeatmapDifficultyMask = _originalAllowedBeatmapDifficultyMask;
            _levelSelectionNavigationController._hidePacksIfOneOrNone = _originalHidePacksIfOneOrNone;

            _levelFilteringNavigationController.Setup(
                SongPackMask.all,
                _originalBeatmapLevelPack,
                _originalLevelCategory,
                false,
                true
            );

            NavigateToBeatmap(_originalBeatmapLevel, _originalLevelCategory);
        }

        #endregion

        #region LevelDetail

        public bool allowDifficultySelection = true;

        private LevelSelectionDetailView _selectionDetailView = null!;
        private GameObject _detailViewButtons = null!;
        private Layout _detailViewCustomButtons = null!;

        private void PatchLevelDetail() {
            if (allowDifficultySelection) {
                return;
            }

            _levelDetailView.gameObject.SetActive(false);
            _selectionDetailView.Enabled = true;
            _selectionDetailView.Refresh();
        }

        private void PatchLevelDetailDynamic() {
            if (!allowDifficultySelection) {
                return;
            }

            _detailViewCustomButtons.ContentTransform.localPosition = _detailViewButtons.transform.localPosition;
            _detailViewButtons.SetActive(false);
            _detailViewCustomButtons.Enabled = true;
        }

        private void UnpatchLevelDetail() {
            if (allowDifficultySelection) {
                _detailViewButtons.SetActive(true);
                _detailViewCustomButtons.Enabled = false;
                return;
            }

            _levelDetailView.gameObject.SetActive(true);
            _selectionDetailView.Enabled = false;
        }

        #endregion

        #region Setup

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            _levelDetailViewController.didChangeContentEvent += HandleContentChanged;

            PatchLevelDetail();
            PatchLevelNavigationController();

            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            NavigateToBeatmap(null, _lastSelectedLevelCategory);
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            _levelDetailViewController.didChangeContentEvent -= HandleContentChanged;

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
            if (category is not SelectLevelCategoryViewController.LevelCategory.None) {
                var cell = _levelCategorySegmentedControl.cells[(int)category - 1];
                cell.SetSelected(true, SelectableCell.TransitionType.Instant, cell, true);
            }

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
            if (allowDifficultySelection) {
                PatchLevelDetailDynamic();
                return;
            }

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