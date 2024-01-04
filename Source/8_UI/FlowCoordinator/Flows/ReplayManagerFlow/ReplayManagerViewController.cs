using System;
using BeatLeader.Components;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.UI.Hub.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.UI.Hub {
    [ViewDefinition(Plugin.ResourcesPath + ".BSML.FlowCoordinator.Flows.ReplayManagerFlow.ReplayManagerView.bsml")]
    internal class ReplayManagerViewController : BSMLAutomaticViewController {
        #region Injection

        [Inject] private readonly ReplayerMenuLoader _replayerLoader = null!;
        [Inject] private readonly IReplayManager _replayManager = null!;
        [Inject] private readonly IReplaysLoader _replaysLoader = null!;

        #endregion

        #region UI Components

        [UIValue("search-filters-panel"), UsedImplicitly]
        private SearchFiltersPanel _searchFiltersPanel = null!;

        [UIComponent("replay-launch-panel"), UsedImplicitly]
        private BeatmapReplayLaunchPanel _replayPanel = null!;

        private ReplayDetailPanel _replayDetailPanel = null!;

        #endregion

        #region Init

        private void Awake() {
            _searchFiltersPanel = ReeUIComponentV2.Instantiate<SearchFiltersPanel>(transform);
            _replayDetailPanel = ReplayDetailPanel.Instantiate(transform);
            _searchFiltersPanel.Setup(_replayManager);
            _replayDetailPanel.Setup(_replayerLoader);
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (firstActivation) {
                _replayPanel.Setup(_replaysLoader);
                _replayPanel.ReplayFilter = _searchFiltersPanel;
                _replayPanel.DetailPanel = _replayDetailPanel;
                _replaysLoader.StartReplaysLoad();
            }
            _searchFiltersPanel.NotifyContainerStateChanged();
        }

        public override void __DismissViewController(Action finishedCallback, AnimationDirection animationDirection = AnimationDirection.Horizontal, bool immediately = false) {
            _childViewController?.__DismissViewController(null, immediately: true);
            base.__DismissViewController(finishedCallback, animationDirection, immediately);
        }

        #endregion
    }
}