using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Replayer;
using BeatLeader.UI.Hub;
using HMUI;
using Zenject;

namespace BeatLeader {
    internal class ReplayerViewNavigator : IReplayerViewNavigator {
        [Inject] private readonly ReplayManagerFlowCoordinator _replayManagerFlowCoordinator = null!;
        [Inject] private readonly BattleRoyaleFlowCoordinator _battleRoyaleFlowCoordinator = null!;
        [Inject] private readonly ReplayerMenuLoader _replayerMenuLoader = null!;

        public async Task NavigateToReplayAsync(FlowCoordinator flowCoordinator, Replay replay, Player player, bool tryLoadSelectedMap) {
            if (tryLoadSelectedMap) {
                await _replayerMenuLoader.StartReplayFromLeaderboardAsync(replay, player);
            } else {
                await _replayerMenuLoader.StartReplayAsync(replay, player);
            }
        }

        public void NavigateToReplayManager(FlowCoordinator flowCoordinator, IReplayHeader header) {
            flowCoordinator.PresentFlowCoordinator(_replayManagerFlowCoordinator);
            _replayManagerFlowCoordinator.NavigateToReplay(header);
        }

        public void NavigateToBattleRoyale(FlowCoordinator flowCoordinator, IReadOnlyList<IReplayInfo> plays) {
            flowCoordinator.PresentFlowCoordinator(_battleRoyaleFlowCoordinator);
        }
    }
}