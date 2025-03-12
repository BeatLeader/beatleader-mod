using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using HMUI;
using JetBrains.Annotations;

namespace BeatLeader {
    [PublicAPI]
    public interface IReplayerViewNavigator {
        /// <summary>
        /// Either starts the replay or navigates to an external provider (e.g. heck).
        /// </summary>
        /// <param name="flowCoordinator">The flow coordinator to be presented from.</param>
        /// <param name="tryLoadSelectedMap">Should loader prioritize the selected map or not. If False, the loader
        /// will attempt to find the map by hash.</param>
        Task NavigateToReplayAsync(
            FlowCoordinator flowCoordinator,
            Replay replay,
            Player player,
            bool tryLoadSelectedMap
        );

        /// <summary>
        /// Navigates to the replay manager view and highlights the provided replay.
        /// </summary>
        /// <param name="flowCoordinator">The flow coordinator to be presented from.</param>
        /// <param name="header">The replay to highlight.</param>
        void NavigateToReplayManager(
            FlowCoordinator flowCoordinator,
            IReplayHeader header
        );

        /// <summary>
        /// Navigates to the battle royale view with the configured lobby.
        /// </summary>
        /// <param name="flowCoordinator">The flow coordinator to be presented from.</param>
        /// <param name="plays">The plays to use for the lobby.</param>
        void NavigateToBattleRoyale(
            FlowCoordinator flowCoordinator,
            BeatmapLevelWithKey level,
            IReadOnlyCollection<IReplayHeader> plays
        );
    }
}