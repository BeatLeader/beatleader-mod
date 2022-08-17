using System.Linq;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.DataManager {
    public class ExMachinaManager : MonoBehaviour {
        #region Start/OnDestroy

        private void Start() {
            LeaderboardState.ProfileRequest.StateChangedEvent += OnProfileRequestStateChanged;
            LeaderboardState.SelectedBeatmapWasChangedEvent += UpdateRating;

            OnProfileRequestStateChanged(LeaderboardState.ProfileRequest.State);
            UpdateRating(LeaderboardState.SelectedBeatmap);
        }

        private void OnDestroy() {
            LeaderboardState.ProfileRequest.StateChangedEvent -= OnProfileRequestStateChanged;
            LeaderboardState.SelectedBeatmapWasChangedEvent -= UpdateRating;
        }

        #endregion

        #region OnProfileRequestStateChanged

        private bool _exMachinaEnabled;

        private void OnProfileRequestStateChanged(RequestState state) {
            if (state != RequestState.Finished) return;
            var roles = FormatUtils.ParsePlayerRoles(LeaderboardState.ProfileRequest.Result.role);
            _exMachinaEnabled = roles.Any(role => role is PlayerRole.Admin or PlayerRole.RankedTeam or PlayerRole.Tipper or PlayerRole.Supporter or PlayerRole.Sponsor);
        }

        #endregion

        #region UpdateRating

        private Coroutine _exMachinaCoroutine;

        private void UpdateRating(IDifficultyBeatmap beatmap) {
            if (_exMachinaCoroutine != null) {
                StopCoroutine(_exMachinaCoroutine);
                LeaderboardState.ExMachinaRequest.TryNotifyCancelled();
            }

            if (!_exMachinaEnabled || beatmap == null) return;

            var key = LeaderboardKey.FromBeatmap(beatmap);

            LeaderboardState.ExMachinaRequest.NotifyStarted();
            _exMachinaCoroutine = StartCoroutine(HttpUtils.GetData<ExMachinaBasicResponse>(
                    string.Format(BLConstants.EX_MACHINA_BASIC, key.Hash, FormatUtils.DiffIdForDiffName(key.Diff)),
                    result => LeaderboardState.ExMachinaRequest.NotifyFinished(result),
                    reason => LeaderboardState.ExMachinaRequest.NotifyFailed(reason)
                )
            );
        }

        #endregion
    }
}