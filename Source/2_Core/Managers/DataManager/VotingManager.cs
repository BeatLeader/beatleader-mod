using System;
using BeatLeader.API;
using BeatLeader.Manager;
using BeatLeader.Models;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.DataManager {
    [UsedImplicitly]
    internal class VotingManager : IInitializable, IDisposable {
        #region Initialize / Dispose

        public void Initialize() {
            UploadReplayRequest.StateChangedEvent += OnUploadRequestStateChanged;
            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
            LeaderboardEvents.SubmitVoteEvent += SubmitVote;
        }

        public void Dispose() {
            UploadReplayRequest.StateChangedEvent -= OnUploadRequestStateChanged;
            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
            LeaderboardEvents.SubmitVoteEvent -= SubmitVote;
        }

        #endregion

        #region Events
        
        private static void OnUploadRequestStateChanged(WebRequests.IWebRequest<Score> instance, WebRequests.RequestState state, string? failReason) {
            if (state is not WebRequests.RequestState.Finished) return;
            UpdateVoteStatus();
        }

        private static void OnSelectedBeatmapWasChanged(bool selectedAny, LeaderboardKey leaderboardKey, BeatmapKey key, BeatmapLevel level) {
            UpdateVoteStatus();
        }

        private static void SubmitVote(Vote vote) {
            if (!TryGetKey(out var key)) return;
            VoteRequest.Send(key.Hash, key.Diff, key.Mode, vote);
        }

        private static void UpdateVoteStatus() {
            if (!TryGetKey(out var key)) return;
            if (!ProfileManager.TryGetUserId(out var userId)) return;
            VoteStatusRequest.SendRequest(key.Hash, key.Diff, key.Mode, userId);
        }

        #endregion

        #region Utils

        private static bool TryGetKey(out LeaderboardKey key) {
            if (!LeaderboardState.IsAnyBeatmapSelected) {
                key = default;
                return false;
            }

            key = LeaderboardState.SelectedLeaderboardKey;
            return true;
        }

        #endregion
    }
}