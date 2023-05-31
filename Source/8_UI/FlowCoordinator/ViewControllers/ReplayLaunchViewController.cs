using System;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.ViewControllers {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.FlowCoordinator.ReplayLaunchView.bsml")]
    internal class ReplayLaunchViewController : BSMLAutomaticViewController {
        #region Injection

        [Inject] private readonly LevelSelectionNavigationController _levelSelectionNavigationController = null!;
        [Inject] private readonly LevelCollectionViewController _levelCollectionViewController = null!;
        [Inject] private readonly StandardLevelDetailViewController _standardLevelDetailViewController = null!;
        [Inject] private readonly BeatLeaderFlowCoordinator _beatLeaderFlowCoordinator = null!;
        [Inject] private readonly ReplayerMenuLoader _replayerLoader = null!;

        #endregion

        #region UI Components

        [UIValue("search-filters-panel"), UsedImplicitly]
        private SearchFiltersPanel _searchFiltersPanel = null!;

        [UIValue("replay-launch-panel"), UsedImplicitly]
        private BeatmapReplayLaunchPanel _replayPanel = null!;

        #endregion

        #region Init

        private void Awake() {
            _replayPanel = ReeUIComponentV2.Instantiate<BeatmapReplayLaunchPanel>(transform);
            _searchFiltersPanel = ReeUIComponentV2.Instantiate<SearchFiltersPanel>(transform);
            _replayPanel.Setup(ReplayManager.Instance, _replayerLoader);
            _searchFiltersPanel.Setup(this,
                _beatLeaderFlowCoordinator,
                _levelSelectionNavigationController,
                _levelCollectionViewController,
                _standardLevelDetailViewController);
            _searchFiltersPanel.SearchDataChangedEvent += HandleSearchDataChanged;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation) _replayPanel.SetBeatmap(null, false, true);
        }

        public override void __DismissViewController(Action finishedCallback, AnimationDirection animationDirection = AnimationDirection.Horizontal, bool immediately = false) {
            _childViewController?.__DismissViewController(null, immediately: true);
            base.__DismissViewController(finishedCallback, animationDirection, immediately);
        }

        #endregion

        #region Callbacks

        private void HandleSearchDataChanged(string searchPrompt, FiltersMenu.FiltersData filters) {
            var forceUpdate = filters is { overrideBeatmap: true, previewBeatmapLevel: null };
            _replayPanel.SetBeatmap(filters.previewBeatmapLevel, filters.overrideBeatmap, forceUpdate);
            if (forceUpdate) return;
            var prompt = searchPrompt.ToLower();
            var diff = filters.beatmapDifficulty;
            var characteristic = filters.beatmapCharacteristic?.serializedName;
            var hasNoFilters = string.IsNullOrEmpty(prompt)
                && string.IsNullOrEmpty(characteristic)
                && !diff.HasValue;
            _replayPanel.Filter(hasNoFilters ? null : SearchPredicate);

            bool SearchPredicate(IReplayHeader header) {
                return header.Info is not { } info ||
                    info.playerName.ToLower().Contains(prompt)
                    && (!diff.HasValue || info.difficulty == diff.Value.ToString())
                    && (characteristic is null || info.mode == characteristic);
            }
        }

        #endregion
    }
}