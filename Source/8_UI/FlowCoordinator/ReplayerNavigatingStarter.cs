using BeatLeader.Models;
using BeatLeader.Models.Replay;
using HMUI;

namespace BeatLeader {
    internal class ReplayerNavigatingStarter : IReplayerStarter {
        public ReplayerNavigatingStarter(FlowCoordinator flowCoordinator, bool alternativeLoading, IReplayerViewNavigator viewNavigator) {
            _flowCoordinator = flowCoordinator;
            _alternativeLoading = alternativeLoading;
            _viewNavigator = viewNavigator;
        }

        private readonly FlowCoordinator _flowCoordinator;
        private readonly bool _alternativeLoading;
        private readonly IReplayerViewNavigator _viewNavigator;

        public async void StartReplay(Replay replay, Player player) {
            await _viewNavigator.NavigateToReplayAsync(_flowCoordinator, replay, player, _alternativeLoading);
        }
    }
}