using System;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.UI.Hub.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using BeatLeader.Utils;
using HMUI;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class ReplayManagerViewController : ViewController {
        #region Injection

        [Inject] private readonly ReplayerMenuLoader _replayerLoader = null!;
        [Inject] private readonly IReplayManager _replayManager = null!;
        [Inject] private readonly IReplaysLoader _replaysLoader = null!;
        [Inject] private readonly LevelSelectionFlowCoordinator _levelSelectionFlowCoordinator = null!;
        [Inject] private readonly ReplayManagerFlowCoordinator _replayManagerFlowCoordinator = null!;

        #endregion

        #region UI Components

        private BeatmapReplayLaunchPanel _replayPanel = null!;
        private ReplayDetailPanel _replayDetailPanel = null!;

        #endregion

        #region Init

        private void Awake() {
            var tagManager = _replayManager.MetadataManager.TagManager;
            new Dummy {
                Children = {
                    new ListFiltersPanel<IReplayHeaderBase> {
                        SearchContract = x => {
                            var info = x.ReplayInfo;
                            var str = new[] { info.PlayerName, info.SongName };
                            return str;
                        },
                        Filters = {
                            new TagFilter().With(x => x.Setup(tagManager))
                        }
                    }.With(
                        x => {
                            //adding beatmap filter items
                            var beatmapFilters = new BeatmapFilterHost();
                            beatmapFilters.Setup(_replayManagerFlowCoordinator, _levelSelectionFlowCoordinator);
                            x.Filters.AddRange(beatmapFilters.Filters);
                        }
                    ).AsFlexItem(
                        size: new() { x = 100f, y = 8f },
                        alignSelf: Align.Center
                    ).Export(out var filtersPanel),
                    //
                    new BeatmapReplayLaunchPanel()
                        .AsFlexItem(basis: 68f)
                        .Bind(ref _replayPanel)
                        .With(x => x.ReplaysList.Filter = filtersPanel)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.Center,
                gap: 2f
            ).WithRectExpand().Use(transform);

            _replayDetailPanel = new ReplayDetailPanel();
            _replayDetailPanel.Setup(_replayerLoader, tagManager);
            _replayPanel.Setup(_replaysLoader);
            _replayPanel.DetailPanel = _replayDetailPanel;
        }

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
            if (!firstActivation) return;
            _replaysLoader.StartReplaysLoad();
        }

        public override void __DismissViewController(Action finishedCallback, AnimationDirection animationDirection = AnimationDirection.Horizontal, bool immediately = false) {
            _childViewController?.__DismissViewController(null, immediately: true);
            base.__DismissViewController(finishedCallback, animationDirection, immediately);
        }

        #endregion
    }
}