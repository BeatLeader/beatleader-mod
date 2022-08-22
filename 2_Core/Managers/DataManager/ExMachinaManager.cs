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
            LeaderboardState.SelectedBeatmapWasChangedEvent += UpdateRating;

            OnRolesUpdated(ProfileManager.Roles);
            UpdateRating(LeaderboardState.SelectedBeatmap);
        }

        public void Dispose() {
            ProfileManager.RolesUpdatedEvent -= OnRolesUpdated;
            LeaderboardState.SelectedBeatmapWasChangedEvent -= UpdateRating;
        }

        #endregion

        #region Events

        private bool _exMachinaEnabled;

        private void OnRolesUpdated(PlayerRole[] playerRoles) {
            _exMachinaEnabled = playerRoles.Any(role => role.IsAnyAdmin() || role.IsAnyRT() || role.IsAnySupporter());
        }

        private void UpdateRating(IDifficultyBeatmap beatmap) {
            if (!_exMachinaEnabled || beatmap == null) return;
            var key = LeaderboardKey.FromBeatmap(beatmap);
            ExMachinaRequest.SendRequest(key);
        }

        #endregion
    }
}