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

        #endregion

        #region UI Components

        private BeatmapReplayLaunchPanel _replayPanel = null!;
        private ReplayDetailPanel _replayDetailPanel = null!;

        #endregion

        #region Init

        private void Awake() {
            ListFiltersPanel<IReplayHeader> filtersPanel = null!;
            new Dummy {
                Children = {
                    new ListFiltersPanel<IReplayHeader> {
                        SearchContract = x => {
                            var info = x.ReplayInfo;
                            var str = new[] { info.PlayerName, info.SongName };
                            return str;
                        },
                        Filters = {
                            new TagFilter().With(
                                x => x.Setup(_replayManager.MetadataManager.TagManager)
                            )
                        }
                    }.With(
                        x => {
                            //adding beatmap filter items
                            var beatmapFilters = new BeatmapFilterHost();
                            beatmapFilters.Setup(this);
                            x.Filters.AddRange(beatmapFilters.Filters);
                        }
                    ).AsFlexItem(basis: 8f).Bind(ref filtersPanel),
                    //
                    new ReeWrapperV3<BeatmapReplayLaunchPanel>()
                        .AsFlexItem(basis: 63f)
                        .BindRee(ref _replayPanel).With(
                            x => {
                                filtersPanel.List = x.ReeComponent.ReplaysList;
                            }
                        )
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.Center,
                gap: 2f
            ).Use(transform);

            _replayDetailPanel = ReplayDetailPanel.Instantiate(transform);
            _replayDetailPanel.Setup(_replayerLoader);
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