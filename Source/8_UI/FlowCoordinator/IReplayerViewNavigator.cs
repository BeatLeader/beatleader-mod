using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using HMUI;

namespace BeatLeader {
    internal interface IReplayerViewNavigator {
        Task NavigateToReplayAsync(
            FlowCoordinator flowCoordinator,
            Replay replay, 
            Player player,
            bool tryAlternativeLoading
        );
    }
}