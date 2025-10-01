using System;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.DataManager {
    public class LeaderboardInfoManager : MonoBehaviour {
        #region Start

        private static TaskCompletionSource<bool>? _taskSource;
        
        public static Task RefreshTask() {
            return _taskSource?.Task ?? Task.CompletedTask;
        }

        private void Start() {
            _taskSource = new();
            FullCacheUpdate().RunCatching();
            LeaderboardState.AddSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
            LeaderboardRequest.StateChangedEvent += LeaderboardRequest_StateChangedEvent;
        }

        private void OnDestroy() {
            LeaderboardState.RemoveSelectedBeatmapListener(OnSelectedBeatmapWasChanged);
            LeaderboardRequest.StateChangedEvent -= LeaderboardRequest_StateChangedEvent;
        }

        #endregion

        #region OnSelectedBeatmapWasChanged

        private string _lastSelectedHash;

        private void OnSelectedBeatmapWasChanged(bool selectedAny, LeaderboardKey leaderboardKey, BeatmapKey key, BeatmapLevel level) {
            if (!selectedAny || leaderboardKey.Hash.Equals(_lastSelectedHash)) return;
            _lastSelectedHash = leaderboardKey.Hash;
            UpdateLeaderboardsByHash(leaderboardKey.Hash);
        }

        private void UpdateLeaderboardsByHash(string hash) {
            LeaderboardRequest.Send(hash);
        }

        private void LeaderboardRequest_StateChangedEvent(WebRequests.IWebRequest<HashLeaderboardsInfoResponse> instance, WebRequests.RequestState state, string? failReason) {
            if (state == WebRequests.RequestState.Finished) {
                var updates = instance.Result.leaderboards.Select(leaderboardInfo => LeaderboardsCache.PutLeaderboardInfo(
                        instance.Result.song,
                        leaderboardInfo.id,
                        leaderboardInfo.difficulty,
                        leaderboardInfo.qualification,
                        leaderboardInfo.clan,
                        leaderboardInfo.clanRankingContested
                        ))
                    .ToArray();

                LeaderboardsCache.NotifyCacheWasChanged(updates);
            } else if (state == WebRequests.RequestState.Failed) {
                Plugin.Log.Debug($"UpdateLeaderboardsByHash failed! {failReason}");
            }
        }

        #endregion

        #region FullCacheUpdate

        private static async Task FullCacheUpdate() {
            var lastTimestamp = LeaderboardsCache.LastCheckTime;
            var newTimestamp = DateTime.UtcNow.ToUnixTime();
            
            const int itemsPerPage = 500;
            var totalPages = 1;
            var page = 1;
            var failed = false;

            void OnSuccess(Paged<MassLeaderboardsInfoResponse> result) {
                foreach (var response in result.data) {
                    LeaderboardsCache.PutLeaderboardInfo(
                        response.song,
                        response.id,
                        response.difficulty,
                        response.qualification,
                        response.clan,
                        response.clanRankingContested
                        );
                }

                totalPages = Mathf.CeilToInt((float) result.metadata.total / result.metadata.itemsPerPage);
                page = result.metadata.page + 1;
            }

            void OnFail(string reason) {
                failed = true;
                Plugin.Log.Debug($"{lastTimestamp} | {page}/{totalPages} | cache update failed! {reason}");
            }

            do {
                var result = await LeaderboardsRequest.Send(lastTimestamp, page, itemsPerPage).Join();
                if (result.RequestState == WebRequests.RequestState.Finished) {
                    OnSuccess(result.Result);
                } else if (result.RequestState == WebRequests.RequestState.Failed) {
                    OnFail(result.FailReason);
                }
            } while (!failed && page <= totalPages);

            _taskSource!.SetResult(true);

            if (!failed) LeaderboardsCache.LastCheckTime = newTimestamp;
            LeaderboardsCache.NotifyCacheWasChanged();
        }

        #endregion
    }
}