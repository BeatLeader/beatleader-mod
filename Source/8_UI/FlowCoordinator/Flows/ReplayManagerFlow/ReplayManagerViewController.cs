using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using HarmonyLib;
using HMUI;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using Zenject;
using CollectionExtensions = Reactive.CollectionExtensions;

namespace BeatLeader.UI.Hub {
    [HarmonyPatch]
    internal class ReplayManagerViewController : ViewController {
        #region Injection

        [Inject] private readonly ReplayerMenuLoader _replayerLoader = null!;
        [Inject] private readonly ReplayPreviewLoader _previewLoader = null!;
        [Inject] private readonly LevelSelectionFlowCoordinator _levelSelectionFlowCoordinator = null!;
        [Inject] private readonly ReplayManagerFlowCoordinator _replayManagerFlowCoordinator = null!;
        [Inject] private readonly BeatLeaderHubTheme _hubTheme = null!;

        #endregion

        #region UI Components

        private BeatmapReplayLaunchPanel _replayPanel = null!;
        private ReplayDetailPanel _replayDetailPanel = null!;

        #endregion

        #region Init

        // TODO: rework, the replay manager should load replays on the background thread
        // TODO: and expose an API to push the action after replays loaded
        public async void NavigateToReplay(IReplayHeader header) {
            await ReplayManager.WaitForLoadingAsync();
            
            _replayPanel.ReplaysListPanel.QueueNavigation(header);
        }

        private void Awake() {
            new Layout {
                Children = {
                    new ListFiltersPanel<IReplayHeader> {
                        SearchContract = x => {
                            var info = x.ReplayInfo;
                            var str = new[] { info.PlayerName, info.SongName };
                            return str;
                        },
                        Filters = {
                            new TagFilter()
                        }
                    }.With(
                        x => {
                            //adding beatmap filter items
                            var beatmapFilters = new BeatmapFilterHost();
                            beatmapFilters.Setup(_replayManagerFlowCoordinator, _levelSelectionFlowCoordinator);
                            CollectionExtensions.AddRange(x.Filters, beatmapFilters.Filters);
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
            _replayDetailPanel.Setup(_replayerLoader);
            _replayPanel.Setup(_previewLoader);
            _replayPanel.DetailPanel = _replayDetailPanel;
            _replayPanel.ReplaysList.Setup(_hubTheme.ReplayManagerSearchTheme);
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

            if (!firstActivation) {
                return;
            }

            ReplayManager.StartLoadingIfNeverLoaded();
        }

        [HarmonyPatch(typeof(ViewController), "__DismissViewController"), HarmonyPrefix]
        private static void DismissViewControllerPrefix(ViewController __instance) {
            if (__instance is not ReplayManagerViewController viewController) return;
            viewController._childViewController?.__DismissViewController(null, immediately: true);
        }

        #endregion
    }
}