using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using HMUI;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleReplaySelectionViewController : ViewController {
        #region Injection

        [Inject] private readonly IReplayManager _replayManager = null!;
        [Inject] private readonly IReplaysLoader _replaysLoader = null!;
        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;

        #endregion

        #region Setup

        private BeatmapReplayLaunchPanel _replayLaunchPanel = null!;

        private void Awake() {
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
                    }.AsFlexItem(basis: 8f).Export(out var filtersPanel),
                    //
                    new ReeWrapperV3<BeatmapReplayLaunchPanel>()
                        .AsFlexItem(basis: 63f)
                        .BindRee(ref _replayLaunchPanel)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.Center,
                gap: 2f
            ).Use(transform);

            var detailPanel = BattleRoyaleDetailPanel.Instantiate(transform);
            _replayLaunchPanel.Setup(_replaysLoader);
            _replayLaunchPanel.DetailPanel = detailPanel;
            _replayLaunchPanel.ReplaysList.Filter = new FilterProxy<IReplayHeader> {
                Filters = {
                    _battleRoyaleHost.ReplayFilter,
                    filtersPanel
                }
            };

            _replayLaunchPanel.ReplaySelectedEvent += HandleReplaySelected;
            _replayLaunchPanel.ReplayDeselectedEvent += HandleReplayDeselected;

            _battleRoyaleHost.ReplayAddedEvent += HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent += HandleReplayRemoved;
            _battleRoyaleHost.ReplayBeatmapChangedEvent += HandleBeatmapChanged;
            _battleRoyaleHost.ReplayNavigationRequestedEvent += HandleHostNavigationRequested;
            
            _replaysLoader.StartReplaysLoad();
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _battleRoyaleHost.ReplayAddedEvent -= HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent -= HandleReplayRemoved;
            _battleRoyaleHost.ReplayBeatmapChangedEvent -= HandleBeatmapChanged;
            _battleRoyaleHost.ReplayNavigationRequestedEvent -= HandleHostNavigationRequested;
        }

        #endregion

        #region Callbacks

        private void HandleHostNavigationRequested(IBattleRoyaleQueuedReplay replay) {
            var list = _replayLaunchPanel.ReplaysList;
            var index = list.Items.FindIndex(x => x.Equals(replay.ReplayHeader));
            list.ScrollTo(index);
            list.Select(index);
        }
        
        private void HandleReplayAdded(IBattleRoyaleQueuedReplay replay, object caller) {
            if (caller.Equals(this)) return;
            _replayLaunchPanel.AddSelectedReplay(replay.ReplayHeader, false);
        }

        private void HandleReplayRemoved(IBattleRoyaleQueuedReplay replay, object caller) {
            if (caller.Equals(this)) return;
            _replayLaunchPanel.RemoveSelectedReplay(replay.ReplayHeader, false);
        }

        private void HandleReplaySelected(IReplayHeaderBase header) {
            _battleRoyaleHost.AddReplay(header, this);
        }

        private void HandleReplayDeselected(IReplayHeaderBase header) {
            _battleRoyaleHost.RemoveReplay(header, this);
        }

        private void HandleBeatmapChanged(IDifficultyBeatmap? beatmap) {
            _replayLaunchPanel.ClearSelectedReplays();
        }

        #endregion
    }
}