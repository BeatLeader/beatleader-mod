using System;
using System.Collections.Generic;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using LeaderboardCore.Interfaces;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class LeaderboardManager : MonoBehaviour, INotifyLeaderboardSet {
        private ScoresScope _selectedScoreScope;
        private ScoresContext _selectedScoreContext;
        private int _lastSelectedPage = 1;
        private IDifficultyBeatmap _lastSelectedBeatmap;

        private Coroutine _scoresTask;

        #region Initialize/Dispose section

        public void Start() {
            SetFakeBloomProperty();

            LeaderboardState.UploadRequest.FinishedEvent += OnUploadSuccess;
            PluginConfig.ScoresContextChangedEvent += ChangeScoreContext;
            LeaderboardState.ScoresScopeChangedEvent += ChangeScoreProvider;

            LeaderboardEvents.UpButtonWasPressedAction += FetchPreviousPage;
            LeaderboardEvents.AroundButtonWasPressedAction += SeekAroundMePage;
            LeaderboardEvents.DownButtonWasPressedAction += FetchNextPage;

            _selectedScoreContext = PluginConfig.ScoresContext;
            _selectedScoreScope = LeaderboardState.ScoresScope;
        }

        private void OnDestroy() {
            LeaderboardState.UploadRequest.FinishedEvent -= OnUploadSuccess;
            PluginConfig.ScoresContextChangedEvent -= ChangeScoreContext;
            LeaderboardState.ScoresScopeChangedEvent -= ChangeScoreProvider;

            LeaderboardEvents.UpButtonWasPressedAction -= FetchPreviousPage;
            LeaderboardEvents.AroundButtonWasPressedAction -= SeekAroundMePage;
            LeaderboardEvents.DownButtonWasPressedAction -= FetchNextPage;
        }

        #endregion

        #region SetGlobalBloomShaderParameter

        private static readonly int FakeBloomAmountPropertyID = Shader.PropertyToID("_FakeBloomAmount");

        private static void SetFakeBloomProperty() {
            bool enableFakeBloom;

            try {
                var mainSettingsModel = Resources.FindObjectsOfTypeAll<MainSettingsModelSO>()[0];
                enableFakeBloom = mainSettingsModel.mainEffectGraphicsSettings.value == 0;
            } catch (Exception) {
                enableFakeBloom = false;
            }

            Shader.SetGlobalFloat(FakeBloomAmountPropertyID, enableFakeBloom ? 1.0f : 0.0f);
        }

        #endregion

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap) {
            Plugin.Log.Debug($"Selected beatmap: {difficultyBeatmap.level.songName}, diff: {difficultyBeatmap.difficulty}");
            _lastSelectedBeatmap = difficultyBeatmap;
            _lastSelectedPage = 1;

            LoadScores();
        }

        #region Score fetching

        private void LoadScores() {
            if (_scoresTask != null) {
                StopCoroutine(_scoresTask);
                LeaderboardState.ScoresRequest.TryNotifyCancelled();
            }

            LeaderboardState.ScoresRequest.NotifyStarted();

            if (BLContext.NoPlayerData) {
                return;
            }

            var userId = BLContext.profile.id;

            var hash = _lastSelectedBeatmap.level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
            var diff = _lastSelectedBeatmap.difficulty.ToString();
            var mode = _lastSelectedBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var scope = _selectedScoreScope.ToString().ToLowerInvariant();
            var context = _selectedScoreContext.ToString().ToLower();

            _scoresTask = StartCoroutine(HttpUtils.GetPagedData<Score>(
                string.Format(BLConstants.SCORES_BY_HASH_PAGED,
                    hash,
                    diff,
                    mode,
                    context,
                    scope,
                    HttpUtils.ToHttpParams(new Dictionary<string, object> {
                        {BLConstants.Param.PLAYER, userId},
                        {BLConstants.Param.COUNT, BLConstants.SCORE_PAGE_SIZE},
                        {BLConstants.Param.PAGE, _lastSelectedPage}
                    })),
                paged =>
                {
                    _lastSelectedPage = paged.metadata.page;
                    LeaderboardState.ScoresRequest.NotifyFinished(paged);
                },
                reason => { LeaderboardState.ScoresRequest.NotifyFailed(reason); }));
        }

        private void SeekScores() {
            if (_scoresTask != null) {
                StopCoroutine(_scoresTask);
                LeaderboardState.ScoresRequest.TryNotifyCancelled();
            }

            LeaderboardState.ScoresRequest.NotifyStarted();

            if (BLContext.NoPlayerData) {
                return;
            }

            var userId = BLContext.profile.id;

            var hash = _lastSelectedBeatmap.level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
            var diff = _lastSelectedBeatmap.difficulty.ToString();
            var mode = _lastSelectedBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var scope = _selectedScoreScope.ToString().ToLowerInvariant();
            var context = _selectedScoreContext.ToString().ToLower();

            _scoresTask = StartCoroutine(HttpUtils.GetPagedData<Score>(
                string.Format(BLConstants.SCORES_BY_HASH_SEEK,
                    hash,
                    diff,
                    mode,
                    context,
                    scope,
                    HttpUtils.ToHttpParams(new Dictionary<string, object> {
                        {BLConstants.Param.PLAYER, userId},
                        {BLConstants.Param.COUNT, BLConstants.SCORE_PAGE_SIZE}
                    })),
                paged =>
                {
                    _lastSelectedPage = paged.metadata.page;
                    LeaderboardState.ScoresRequest.NotifyFinished(paged);
                },
                reason => { LeaderboardState.ScoresRequest.NotifyFailed(reason); }));
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