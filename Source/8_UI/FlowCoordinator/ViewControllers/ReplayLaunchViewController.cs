using System;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using JetBrains.Annotations;
using UnityEngine;
using Zenject;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.FlowCoordinator.ReplayLaunchView.bsml")]
    internal class ReplayLaunchViewController : BSMLAutomaticViewController {
        #region Injection

        [Inject] private readonly LevelSelectionNavigationController _levelSelectionNavigationController = null!;
        [Inject] private readonly LevelCollectionViewController _levelCollectionViewController = null!;
        [Inject] private readonly StandardLevelDetailViewController _standardLevelDetailViewController = null!;
        [Inject] private readonly BeatLeaderFlowCoordinator _beatLeaderFlowCoordinator = null!;
        [Inject] private readonly IReplayerViewNavigator _replayerNavigator = null!;

        #endregion

        #region UI Components

        [UIValue("search-filters-panel"), UsedImplicitly]
        private SearchFiltersPanel _searchFiltersPanel = null!;

        [UIValue("replay-launch-panel"), UsedImplicitly]
        private BeatmapReplayLaunchPanel _replayPanel = null!;

        #endregion

        #region Init

        private readonly HarmonyAutoPatch _dismissViewControllerPatch = new HarmonyPatchDescriptor(
            typeof(ViewController).GetMethod(nameof(ViewController.__DismissViewController), ReflectionUtils.DefaultFlags)!,
            typeof(ReplayLaunchViewController).GetMethod(nameof(DismissViewControllerPrefix), ReflectionUtils.StaticFlags));
        
        private void Awake() {
            _replayPanel = ReeUIComponentV2.Instantiate<BeatmapReplayLaunchPanel>(transform);
            _searchFiltersPanel = ReeUIComponentV2.Instantiate<SearchFiltersPanel>(transform);
            var adapter = new ReplayerNavigatingStarter(_beatLeaderFlowCoordinator, false, _replayerNavigator);
            _replayPanel.Setup(ReplayManager.Instance, adapter);
            _searchFiltersPanel.Setup(
                ReplayManager.Instance,
                this,
                _beatLeaderFlowCoordinator,
                _levelSelectionNavigationController,
                _levelCollectionViewController,
                _standardLevelDetailViewController);
            _searchFiltersPanel.SearchDataChangedEvent += HandleSearchDataChanged;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation) _replayPanel.ReloadData();
            _searchFiltersPanel.NotifyContainerStateChanged();
            _replayPanel.PrepareForDisplay();
        }
        
        public void __DismissViewController(
            Action finishedCallback,
            AnimationDirection animationDirection,
            bool immediately)
        {
            _childViewController?.__DismissViewController(null, immediately: true);
            base.__DismissViewController(finishedCallback, animationDirection, immediately);
        }

        #endregion

        #region Callbacks

        private void HandleSearchDataChanged(string searchPrompt, FiltersMenu.FiltersData filters) {
            var prompt = searchPrompt.ToLower();
            var beatmap = filters.previewBeatmapLevel;
            var diff = filters.beatmapDifficulty;
            var characteristic = filters.beatmapCharacteristic?.serializedName;
            var hasNoFilters = 
                !filters.overrideBeatmap 
                && string.IsNullOrEmpty(prompt)
                && string.IsNullOrEmpty(characteristic)
                && !diff.HasValue;
            //if (filters is { overrideBeatmap: true, previewBeatmapLevel: null }) return;
            _replayPanel.SetFilter(hasNoFilters ? null : SearchPredicate);

            bool SearchPredicate(IReplayHeader header) {
                return header.ReplayInfo is not { } info ||
                    info.PlayerName.ToLower().Contains(prompt)
                    && (beatmap is null || beatmap.levelID.Replace("custom_level_", "") == info.SongHash)
                    && (!diff.HasValue || info.SongDifficulty == diff.Value.ToString())
                    && (characteristic is null || info.SongMode == characteristic);
            }
        }

        private static void DismissViewControllerPrefix(object __instance) {
            if (__instance is not ReplayLaunchViewController { } view) return;
            ((ReplayLaunchViewController)__instance)._replayPanel.PrepareForDismiss();
            view._childViewController?.__DismissViewController(null, immediately: true);
        }

        #endregion
    }
}