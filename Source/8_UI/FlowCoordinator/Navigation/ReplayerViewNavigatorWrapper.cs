using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader {
    /// <summary>
    /// Used to wrap IReplayerViewNavigator and avoid providing FlowCoordinator each time.
    /// </summary>
    [PublicAPI]
    public class ReplayerViewNavigatorWrapper {
        public ReplayerViewNavigatorWrapper(IReplayerViewNavigator replayerViewNavigator, FlowCoordinator flowCoordinator) {
            _viewNavigator = replayerViewNavigator;
            _flowCoordinator = flowCoordinator;
        }

        private readonly IReplayerViewNavigator _viewNavigator;
        private readonly FlowCoordinator _flowCoordinator;

        /// <inheritdoc cref="IReplayerViewNavigator.NavigateToReplayAsync"/>
        public Task NavigateToReplayAsync(Replay replay, Player player, bool tryLoadSelectedMap) {
            return _viewNavigator.NavigateToReplayAsync(_flowCoordinator, replay, player, tryLoadSelectedMap);
        }

        /// <inheritdoc cref="IReplayerViewNavigator.NavigateToReplayManager"/>
        public void NavigateToReplayManager(IReplayHeader header) {
            _viewNavigator.NavigateToReplayManager(_flowCoordinator, header);
        }

        /// <inheritdoc cref="IReplayerViewNavigator.NavigateToBattleRoyale"/>
        public void NavigateToBattleRoyale(IReadOnlyList<IReplayInfo> plays) {
            _viewNavigator.NavigateToBattleRoyale(_flowCoordinator, plays);
        }
    }
}