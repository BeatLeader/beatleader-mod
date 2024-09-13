using BeatLeader.Models;
using BeatLeader.UI.Hub.Models;
using HMUI;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleReplaySelectionViewController : ViewController {
        #region Injection

        [Inject] private readonly IReplayManager _replayManager = null!;
        [Inject] private readonly IReplaysLoader _replaysLoader = null!;
        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;
        [Inject] private readonly BeatLeaderHubTheme _hubTheme = null!;

        #endregion

        #region Setup

        private BeatmapReplayLaunchPanel _replayLaunchPanel = null!;

        private void Awake() {
            new Dummy {
                Children = {
                    new ListFiltersPanel<IReplayHeaderBase> {
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
                    }.AsFlexItem(
                        size: new() { x = 100f, y = 8f },
                        alignSelf: Align.Center
                    ).Export(out var filtersPanel),
                    //
                    new BeatmapReplayLaunchPanel()
                        .AsFlexItem(basis: 68f)
                        .Bind(ref _replayLaunchPanel)
                }
            }.AsFlexGroup(
                direction: FlexDirection.Column,
                justifyContent: Justify.Center,
                gap: 2f
            ).WithRectExpand().Use(transform);

            var detailPanel = new BattleRoyaleDetailPanel();
            _replayLaunchPanel.Setup(_replaysLoader);
            _replayLaunchPanel.DetailPanel = detailPanel;
            _replayLaunchPanel.ReplaysList.Setup(_hubTheme.ReplayManagerSearchTheme);
            _replayLaunchPanel.ReplaysList.Filter = new FilterProxy<IReplayHeaderBase> {
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

        public override void OnDestroy() {
            base.OnDestroy();
            _battleRoyaleHost.ReplayAddedEvent -= HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent -= HandleReplayRemoved;
            _battleRoyaleHost.ReplayBeatmapChangedEvent -= HandleBeatmapChanged;
            _battleRoyaleHost.ReplayNavigationRequestedEvent -= HandleHostNavigationRequested;
        }

        #endregion

        #region Callbacks

        private void HandleHostNavigationRequested(IBattleRoyaleReplay replay) {
            var list = _replayLaunchPanel.ReplaysList;
            list.ScrollTo(replay.ReplayHeader);
            list.Select(replay.ReplayHeader);
        }
        
        private void HandleReplayAdded(IBattleRoyaleReplay replay, object caller) {
            if (caller.Equals(this)) return;
            _replayLaunchPanel.AddSelectedReplay(replay.ReplayHeader, false);
        }

        private void HandleReplayRemoved(IBattleRoyaleReplay replay, object caller) {
            if (caller.Equals(this)) return;
            _replayLaunchPanel.RemoveSelectedReplay(replay.ReplayHeader, false);
        }

        private void HandleReplaySelected(IReplayHeaderBase header) {
            _battleRoyaleHost.AddReplay(header, this);
        }

        private void HandleReplayDeselected(IReplayHeaderBase header) {
            _battleRoyaleHost.RemoveReplay(header, this);
        }

        private void HandleBeatmapChanged(BeatmapLevelWithKey beatmap) {
            _replayLaunchPanel.ClearSelectedReplays();
        }

        #endregion
    }
}