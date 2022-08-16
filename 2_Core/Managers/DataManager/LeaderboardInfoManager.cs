using System.Collections;
using BeatLeader.Models;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class LeaderboardInfoManager : MonoBehaviour {
        #region Start

        private void Start() {
            StartCoroutine(FullCacheUpdateTask());

            LeaderboardState.SelectedBeatmapWasChangedEvent += OnSelectedBeatmapWasChanged;
            OnSelectedBeatmapWasChanged(LeaderboardState.SelectedBeatmap);
        }

        private void OnDestroy() {
            LeaderboardState.SelectedBeatmapWasChangedEvent -= OnSelectedBeatmapWasChanged;
        }

        #endregion

        #region OnSelectedBeatmapWasChanged

        private string _lastSelectedHash;

        private void OnSelectedBeatmapWasChanged([CanBeNull] IDifficultyBeatmap beatmap) {
            if (beatmap == null) return;
            var leaderboardKey = LeaderboardKey.FromBeatmap(beatmap);
            if (leaderboardKey.Hash.Equals(_lastSelectedHash)) return;
            _lastSelectedHash = leaderboardKey.Hash;
            UpdateLeaderboardsByHash(leaderboardKey.Hash);
        }

        private Coroutine _selectedLeaderboardUpdateCoroutine;

        private void UpdateLeaderboardsByHash(string hash) {
            if (_selectedLeaderboardUpdateCoroutine != null) StopCoroutine(_selectedLeaderboardUpdateCoroutine);

            _selectedLeaderboardUpdateCoroutine = StartCoroutine(HttpUtils.GetData<HashLeaderboardsInfoResponse>(
                    string.Format(BLConstants.LEADERBOARDS_HASH, hash),
                    result => {
                        foreach (var leaderboardInfo in result.leaderboards) {
                            LeaderboardsCache.PutLeaderboardInfo(result.song, leaderboardInfo.id, leaderboardInfo.difficulty, leaderboardInfo.qualification);
                        }

                        LeaderboardsCache.NotifyCacheWasChanged();
                    },
                    reason => Plugin.Log.Debug($"UpdateLeaderboardsByHash failed! {reason}")
                )
            );
        }

        #endregion

        #region FullCacheUpdate

        private static IEnumerator FullCacheUpdateTask() {
            yield return UpdateLeaderboards("nominated");
            yield return UpdateLeaderboards("qualified");
            yield return UpdateLeaderboards("ranked");
            LeaderboardsCache.NotifyCacheWasChanged();
        }

        private static IEnumerator UpdateLeaderboards(string type) {
            const int itemsPerPage = 500;
            var totalPages = 1;
            var page = 1;
            var failed = false;

            do {
                yield return HttpUtils.GetPagedData<MassLeaderboardsInfoResponse>(
                    string.Format(BLConstants.MASS_LEADERBOARDS, page, itemsPerPage, type),
                    result => {
                        foreach (var response in result.data) {
                            LeaderboardsCache.PutLeaderboardInfo(response.song, response.id, response.difficulty, response.qualification);
                        }

                        totalPages = result.metadata.total / result.metadata.itemsPerPage;
                        page = result.metadata.page + 1;
                    },
                    reason => {
                        failed = true;
                        Plugin.Log.Debug($"{type} {page}/{totalPages} cache update failed! {reason}");
                    });
            } while (!failed && page < totalPages);
        }

        #endregion
    }
}