using System;
using BeatLeader.API.Methods;
using BeatLeader.Manager;
using BeatLeader.Models;
using LeaderboardCore.Interfaces;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class LeaderboardManager : MonoBehaviour, INotifyLeaderboardSet {
        #region Properties

        private ScoresScope _selectedScoreScope;
        private ScoresContext _selectedScoreContext;
        private int _lastSelectedPage = 1;
        private IDifficultyBeatmap _lastSelectedBeatmap;

        private string Hash => _lastSelectedBeatmap.level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
        private string Diff => _lastSelectedBeatmap.difficulty.ToString();
        private string Mode => _lastSelectedBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
        private string Scope => _selectedScoreScope.ToString().ToLowerInvariant();
        private string Context => _selectedScoreContext.ToString().ToLower();

        #endregion

        #region Initialize/Dispose section

        public void Start() {
            SetFakeBloomProperty();
            
            ScoresRequest.AddStateListener(OnScoresRequestStateChanged);
            LeaderboardState.UploadRequest.StartedEvent += OnUploadStarted;
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
            ScoresRequest.RemoveStateListener(OnScoresRequestStateChanged);
            LeaderboardState.UploadRequest.StartedEvent -= OnUploadStarted;
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

            LeaderboardState.SelectedBeatmap = difficultyBeatmap;
        }

        #region Upload

        private LeaderboardKey _uploadLeaderboardKey;

        private void OnUploadStarted() {
            _uploadLeaderboardKey = LeaderboardKey.FromBeatmap(_lastSelectedBeatmap);
        }

        private void OnUploadSuccess(Score score) {
            if (!_uploadLeaderboardKey.Equals(LeaderboardKey.FromBeatmap(_lastSelectedBeatmap))) return;
            LoadScores();
        }

        #endregion

        #region Score fetching

        private void LoadScores() {
            if (!ProfileManager.TryGetUserId(out var userId)) return;
            ScoresRequest.SendPageRequest(userId, Hash, Diff, Mode, Context, Scope, _lastSelectedPage);
        }

        private void SeekScores() {
            if (!ProfileManager.TryGetUserId(out var userId)) return;
            ScoresRequest.SendSeekRequest(userId, Hash, Diff, Mode, Context, Scope);
        }

        private void OnScoresRequestStateChanged(API.RequestState state, Paged<Score> result, string failReason) {
            if (state is not API.RequestState.Finished) return;
            _lastSelectedPage = result.metadata.page;
        }

        #endregion

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