using System;
using BeatLeader.API.Methods;
using BeatLeader.Manager;
using BeatLeader.Models;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.DataManager {
    [UsedImplicitly]
    internal class VotingManager : IInitializable, IDisposable {
        #region Initialize / Dispose

        public void Initialize() {
            UploadReplayRequest.AddStateListener(OnUploadRequestStateChanged);
            LeaderboardState.SelectedBeatmapWasChangedEvent += OnSelectedBeatmapWasChanged;
            LeaderboardEvents.SubmitVoteEvent += SubmitVote;

            OnSelectedBeatmapWasChanged(LeaderboardState.SelectedBeatmap);
        }

        public void Dispose() {
            UploadReplayRequest.RemoveStateListener(OnUploadRequestStateChanged);
            LeaderboardState.SelectedBeatmapWasChangedEvent -= OnSelectedBeatmapWasChanged;
            LeaderboardEvents.SubmitVoteEvent -= SubmitVote;
        }

        #endregion

        #region Events
        
        private static void OnUploadRequestStateChanged(API.RequestState state, Score result, string failReason) {
            if (state is not API.RequestState.Finished) return;
            UpdateVoteStatus();
        }

        private static void OnSelectedBeatmapWasChanged(IDifficultyBeatmap beatmap) {
            UpdateVoteStatus();
        }

        private static void SubmitVote(Vote vote) {
            if (!TryGetKey(out var key)) return;
            VoteRequest.SendRequest(key.Hash, key.Diff, key.Mode, vote);
        }

        private static void UpdateVoteStatus() {
            if (!TryGetKey(out var key)) return;
            if (!ProfileManager.TryGetUserId(out var userId)) return;
            VoteStatusRequest.SendRequest(key.Hash, key.Diff, key.Mode, userId);
        }

        #endregion

        #region Utils

        private static bool TryGetKey(out LeaderboardKey key) {
            if (LeaderboardState.SelectedBeatmap == null) {
                key = default;
                return false;
            }

            key = LeaderboardKey.FromBeatmap(LeaderboardState.SelectedBeatmap);
            return true;
        }

        #endregion
    }
}