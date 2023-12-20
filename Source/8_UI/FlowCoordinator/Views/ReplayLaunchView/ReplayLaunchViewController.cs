using System;
using BeatLeader.Components;
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

        [Inject] private readonly ReplayerMenuLoader _replayerLoader = null!;

        #endregion

        #region UI Components

        [UIValue("search-filters-panel"), UsedImplicitly]
        private SearchFiltersPanel _searchFiltersPanel = null!;

        [UIComponent("replay-launch-panel"), UsedImplicitly]
        private BeatmapReplayLaunchPanel _replayPanel = null!;

        #endregion

        #region Init

        private void Awake() {
            _searchFiltersPanel = ReeUIComponentV2.Instantiate<SearchFiltersPanel>(transform);
            _searchFiltersPanel.Setup(ReplayManager.Instance);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation) {
                _replayPanel.ReplayModeChangedEvent += HandleReplayModeChangedEvent;
                _replayPanel.Setup(ReplayManager.Instance, _replayerLoader);
                _replayPanel.Filter = _searchFiltersPanel;
                _replayPanel.ReloadData();
            }
            _searchFiltersPanel.NotifyContainerStateChanged();
        }

        public override void __DismissViewController(Action finishedCallback, AnimationDirection animationDirection = AnimationDirection.Horizontal, bool immediately = false) {
            _childViewController?.__DismissViewController(null, immediately: true);
            base.__DismissViewController(finishedCallback, animationDirection, immediately);
        }

        #endregion

        #region Callbacks
        
        private void HandleReplayModeChangedEvent(BeatmapReplayLaunchPanel.ReplayMode mode) {
            _searchFiltersPanel.FilterPanelInteractable = mode is BeatmapReplayLaunchPanel.ReplayMode.Standard;
        }

        #endregion
    }
}