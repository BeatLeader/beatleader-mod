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
            UploadReplayRequest.AddStateListener(OnUploadRequestStateChanged);

            PluginConfig.ScoresContextChangedEvent += ChangeScoreContext;
            LeaderboardState.ScoresScopeChangedEvent += ChangeScoreProvider;
            LeaderboardState.IsVisibleChangedEvent += OnIsVisibleChanged;
            LeaderboardEvents.UpButtonWasPressedAction += FetchPreviousPage;
            LeaderboardEvents.AroundButtonWasPressedAction += SeekAroundMePage;
            LeaderboardEvents.DownButtonWasPressedAction += FetchNextPage;
            LeaderboardEvents.CaptorClanWasClickedEvent += HandleCaptorClanClick;
            ProfileManager.FriendsUpdatedEvent += OnFriendsUpdated;

            _selectedScoreContext = PluginConfig.ScoresContext;
            _selectedScoreScope = LeaderboardState.ScoresScope;
        }

        private void OnDestroy() {
            ScoresRequest.RemoveStateListener(OnScoresRequestStateChanged);
            UploadReplayRequest.RemoveStateListener(OnUploadRequestStateChanged);

            PluginConfig.ScoresContextChangedEvent -= ChangeScoreContext;
            LeaderboardState.ScoresScopeChangedEvent -= ChangeScoreProvider;
            LeaderboardState.IsVisibleChangedEvent -= OnIsVisibleChanged;
            LeaderboardEvents.UpButtonWasPressedAction -= FetchPreviousPage;
            LeaderboardEvents.AroundButtonWasPressedAction -= SeekAroundMePage;
            LeaderboardEvents.DownButtonWasPressedAction -= FetchNextPage;
            LeaderboardEvents.CaptorClanWasClickedEvent -= HandleCaptorClanClick;
            ProfileManager.FriendsUpdatedEvent -= OnFriendsUpdated;
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

        #region TryUpdateScores / OnIsVisibleChanged

        private LeaderboardType _leaderboardType = LeaderboardType.SongDiffPlayerScores;
        private bool _updateRequired;

        private void TryUpdateScores() {
            _updateRequired = true;
            if (!LeaderboardState.IsVisible) return;
            UpdateScores();
        }

        private void OnIsVisibleChanged(bool isVisible) {
            if (!isVisible || !_updateRequired) return;
            UpdateScores();
        }

        private void UpdateScores() {
            _updateRequired = false;

            switch (_leaderboardType) {
                case LeaderboardType.SongDiffPlayerScores: {
                    LoadPlayerScores();
                    break;
                }
                case LeaderboardType.SongDiffClanScores: {
                    LoadClanScores();
                    break;
                }
                default: break;
            }
        }

        #endregion

        #region OnLeaderboardSet

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap) {
            Plugin.Log.Debug($"Selected beatmap: {difficultyBeatmap.level.songName}, diff: {difficultyBeatmap.difficulty}");
            _lastSelectedBeatmap = difficultyBeatmap;
            _lastSelectedPage = 1;

            TryUpdateScores();

            LeaderboardState.SelectedBeatmap = difficultyBeatmap;
        }

        #endregion

        #region OnFriendsUpdated

        private void OnFriendsUpdated() {
            if (_selectedScoreScope is not ScoresScope.Friends) return;
            TryUpdateScores();
        }

        #endregion

        #region OnUploadRequestStateChanged

        private LeaderboardKey _uploadLeaderboardKey;

        private void OnUploadRequestStateChanged(API.RequestState state, Score result, string failReason) {
            if (_lastSelectedBeatmap == null) return;

            switch (state) {
                case API.RequestState.Started:
                    _uploadLeaderboardKey = LeaderboardKey.FromBeatmap(_lastSelectedBeatmap);
                    break;
                case API.RequestState.Finished:
                    if (!_uploadLeaderboardKey.Equals(LeaderboardKey.FromBeatmap(_lastSelectedBeatmap))) return;
                    TryUpdateScores();
                    break;
            }
        }

        #endregion

        #region Score fetching

        private void LoadPlayerScores() {
            if (!ProfileManager.TryGetUserId(out var userId)) return;
            ScoresRequest.SendPlayerScoresPageRequest(userId, Hash, Diff, Mode, Context, Scope, _lastSelectedPage);
        }

        private void SeekPlayerScores() {
            if (!ProfileManager.TryGetUserId(out var userId)) return;
            ScoresRequest.SendPlayerScoresSeekRequest(userId, Hash, Diff, Mode, Context, Scope);
        }

        private void LoadClanScores() {
            ScoresRequest.SendClanScoresPageRequest(Hash, Diff, Mode, _lastSelectedPage);
        }

        private void OnScoresRequestStateChanged(API.RequestState state, ScoresTableContent result, string failReason) {
            if (state is not API.RequestState.Finished || _leaderboardType is not LeaderboardType.SongDiffPlayerScores) return;
            _lastSelectedPage = result.CurrentPage;
        }

        #endregion

        #region Select score scope

        private void ChangeScoreProvider(ScoresScope scope) {
            Plugin.Log.Debug($"Attempt to switch score scope from [{_selectedScoreScope}] to [{scope}]");

            if (_selectedScoreScope != scope) {
                _leaderboardType = LeaderboardType.SongDiffPlayerScores;
                _selectedScoreScope = scope;
                _lastSelectedPage = 1;

                TryUpdateScores();
            }
        }

        #endregion

        #region Select score context

        private void ChangeScoreContext(ScoresContext context) {
            Plugin.Log.Debug($"Attempt to switch score context from [{_selectedScoreContext}] to [{context}]");

            if (_selectedScoreContext != context) {
                _leaderboardType = LeaderboardType.SongDiffPlayerScores;
                _selectedScoreContext = context;
                _lastSelectedPage = 1;

                TryUpdateScores();
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
            TryUpdateScores();
        }

        private void FetchNextPage() {
            _lastSelectedPage++;
            TryUpdateScores();
        }

        private void SeekAroundMePage() {
            SeekPlayerScores();
        }

        private void HandleCaptorClanClick() {
            _leaderboardType = _leaderboardType switch {
                LeaderboardType.SongDiffPlayerScores => LeaderboardType.SongDiffClanScores,
                _ => LeaderboardType.SongDiffPlayerScores
            };
            _lastSelectedPage = 1;
            TryUpdateScores();
        }

        #endregion
    }
}