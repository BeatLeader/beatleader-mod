using System;
using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.DataManager {
    [UsedImplicitly]
    internal class ExMachinaManager : IInitializable, IDisposable {
        #region Initialize / Dispose

        public void Initialize() {
            ProfileManager.RolesUpdatedEvent += OnRolesUpdated;
            OnRolesUpdated(ProfileManager.Roles);
            
            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapChanged);
        }

        public void Dispose() {
            ProfileManager.RolesUpdatedEvent -= OnRolesUpdated;
            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapChanged);
        }

        #endregion

        #region Events

        private bool _exMachinaEnabled;

        private void OnRolesUpdated(PlayerRole[] playerRoles) {
            _exMachinaEnabled = playerRoles.Any(role => role.IsAnyAdmin() || role.IsAnyRT() || role.IsAnySupporter());
        }

        private void OnSelectedBeatmapChanged(bool selectedAny, LeaderboardKey leaderboardKey, IDifficultyBeatmap beatmap) {
            if (!selectedAny || !_exMachinaEnabled) return;
            ExMachinaRequest.SendRequest(leaderboardKey);
        }

        #endregion
    }
}