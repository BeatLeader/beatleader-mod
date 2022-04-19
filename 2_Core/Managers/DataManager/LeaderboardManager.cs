using System;
using System.Collections.Generic;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using LeaderboardCore.Interfaces;
using UnityEngine;
using Zenject;

namespace BeatLeader.DataManager {
    internal class LeaderboardManager : MonoBehaviour, INotifyLeaderboardSet {
        private ScoresScope _selectedScoreScope = ScoresScope.Global;
        private ScoresContext _selectedScoreContext = BLContext.DefaultScoresContext;
        private int _lastSelectedPage = 1;
        private IDifficultyBeatmap _lastSelectedBeatmap;

        private Coroutine _scoresTask;

        #region Initialize/Dispose section

        public void Start() {
            LeaderboardState.UploadRequest.FinishedEvent += OnUploadSuccess;

            LeaderboardEvents.ScopeWasSelectedAction += ChangeScoreProvider;
            LeaderboardEvents.ContextWasSelectedAction += ChangeScoreContext;

            LeaderboardEvents.UpButtonWasPressedAction += FetchPreviousPage;
            LeaderboardEvents.AroundButtonWasPressedAction += SeekAroundMePage;
            LeaderboardEvents.DownButtonWasPressedAction += FetchNextPage;
        }

        private void OnDestroy() {
            LeaderboardState.UploadRequest.FinishedEvent -= OnUploadSuccess;

            LeaderboardEvents.ScopeWasSelectedAction -= ChangeScoreProvider;
            LeaderboardEvents.ContextWasSelectedAction -= ChangeScoreContext;

            LeaderboardEvents.UpButtonWasPressedAction -= FetchPreviousPage;
            LeaderboardEvents.AroundButtonWasPressedAction -= SeekAroundMePage;
            LeaderboardEvents.DownButtonWasPressedAction -= FetchNextPage;
        }

        #endregion

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap) {
            Plugin.Log.Debug($"Seleceted beatmap: {difficultyBeatmap.level.songName}, diff: {difficultyBeatmap.difficulty}");
            _lastSelectedBeatmap = difficultyBeatmap;
            _lastSelectedPage = 1;

            LoadScores();
        }

        #region score fetching

        private void LoadScores() {
            if (_scoresTask != null) {
                StopCoroutine(_scoresTask);
                LeaderboardState.ScoresRequest.TryNotifyCancelled();
            }

            LeaderboardState.ScoresRequest.NotifyStarted();

            string hash = _lastSelectedBeatmap.level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
            string diff = _lastSelectedBeatmap.difficulty.ToString();
            string mode = _lastSelectedBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string scope = _selectedScoreScope.ToString().ToLowerInvariant();
            string context = _selectedScoreContext.ToString().ToLower();

            string userId = BLContext.profile.id;

            _scoresTask = StartCoroutine(HttpUtils.GetPagedData<Score>(
                String.Format(BLConstants.SCORES_BY_HASH_PAGED, hash, diff, mode, context, scope, HttpUtils.ToHttpParams(new Dictionary<string, object> {
                    { BLConstants.Param.PLAYER, userId },
                    { BLConstants.Param.COUNT, BLConstants.SCORE_PAGE_SIZE },
                    { BLConstants.Param.PAGE, _lastSelectedPage }
                })),
                paged => {
                    _lastSelectedPage = paged.metadata.page;
                    LeaderboardState.ScoresRequest.NotifyFinished(paged);
                }, reason => {
                    LeaderboardState.ScoresRequest.NotifyFailed(reason);
                }));
        }

        private void SeekScores() {
            if (_scoresTask != null) {
                StopCoroutine(_scoresTask);
                LeaderboardState.ScoresRequest.TryNotifyCancelled();
            }

            LeaderboardState.ScoresRequest.NotifyStarted();

            string hash = _lastSelectedBeatmap.level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
            string diff = _lastSelectedBeatmap.difficulty.ToString();
            string mode = _lastSelectedBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string scope = _selectedScoreScope.ToString().ToLowerInvariant();
            string context = _selectedScoreContext.ToString().ToLower();

            string userId = BLContext.profile.id;

            _scoresTask = StartCoroutine(HttpUtils.GetPagedData<Score>(
                String.Format(BLConstants.SCORES_BY_HASH_SEEK, hash, diff, mode, context, scope, HttpUtils.ToHttpParams(new Dictionary<string, object> {
                    { BLConstants.Param.PLAYER, userId },
                    { BLConstants.Param.COUNT, BLConstants.SCORE_PAGE_SIZE }
                })),
                paged => {
                    _lastSelectedPage = paged.metadata.page;
                    LeaderboardState.ScoresRequest.NotifyFinished(paged);
                }, reason => {
                    LeaderboardState.ScoresRequest.NotifyFailed(reason);
                }));
        }

        #endregion

        private void OnUploadSuccess(Score score) {
            LoadScores();
        }

        #region Select score scope

        private void ChangeScoreProvider(ScoresScope scope) {
            Plugin.Log.Debug($"Attempt to switch score scope from [{_selectedScoreScope}] to [{scope}]");

            if (_selectedScoreScope != scope) {
                _selectedScoreScope = scope;
                _lastSelectedPage = 1;

                LoadScores();
            }
        }

        #endregion

        #region Select score context

        private void ChangeScoreContext(ScoresContext context) {
            Plugin.Log.Debug($"Attempt to switch score context from [{_selectedScoreContext}] to [{context}]");

            if (_selectedScoreContext != context) {
                _selectedScoreContext = context;
                _lastSelectedPage = 1;

                LoadScores();
            }
        }

        #endregion

        #region Pagination

        private void FetchPreviousPage() {
            if (_lastSelectedPage <= 1) {
                _lastSelectedPage = 1;
                return;
            }
            _lastSelectedPage--;
            LoadScores();
        }

        private void FetchNextPage() {
            _lastSelectedPage++;
            LoadScores();
        }

        private void SeekAroundMePage() {
            SeekScores();
        }

        #endregion
    }
}
