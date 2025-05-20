using System;
using BeatLeader.API;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.WebRequests;
using JetBrains.Annotations;
using LeaderboardCore.Interfaces;
using UnityEngine;
using Zenject;

namespace BeatLeader.DataManager {
    internal class LeaderboardManager : MonoBehaviour, INotifyLeaderboardSet {
        #region Properties

        [Inject, UsedImplicitly]
        private BeatmapLevelsModel _beatmapLevelsModel;

        private ScoresScope _selectedScoreScope;
        private int _selectedScoreContext;
        private int _lastSelectedPage = 1;
        private BeatmapKey _lastSelectedBeatmap;
        private string Scope => _selectedScoreScope.ToString().ToLowerInvariant();
        private string Context => ScoresContexts.ContextForId(_selectedScoreContext).Key;

        #endregion

        #region Initialize/Dispose section

        public void Start() {
            SetFakeBloomProperty();

            ScoresRequest.StateChangedEvent += OnScoresRequestStateChanged;
            ClanScoresRequest.StateChangedEvent += OnScoresRequestStateChanged;

            UploadReplayRequest.StateChangedEvent += OnUploadRequestStateChanged;
            UserRequest.StateChangedEvent += OnUserRequestStateChanged;

            PluginConfig.ScoresContextChangedEvent += OnScoresContextWasChanged;
            LeaderboardState.ScoresScopeChangedEvent += OnScoresScopeWasSelected;
            LeaderboardState.IsVisibleChangedEvent += OnIsVisibleChanged;
            LeaderboardEvents.UpButtonWasPressedAction += OnPreviousPageClick;
            LeaderboardEvents.AroundButtonWasPressedAction += OnAroundMeClick;
            LeaderboardEvents.DownButtonWasPressedAction += OnNextPageClick;
            LeaderboardEvents.CaptorClanWasClickedEvent += OnCaptorClanClick;
            ProfileManager.FriendsUpdatedEvent += OnFriendsUpdated;
            LeaderboardsCache.CacheWasChangedEvent += OnCacheUpdated;

            _selectedScoreContext = PluginConfig.ScoresContext;
            _selectedScoreScope = LeaderboardState.ScoresScope;
        }

        private void OnDestroy() {
            ScoresRequest.StateChangedEvent -= OnScoresRequestStateChanged;
            ClanScoresRequest.StateChangedEvent -= OnScoresRequestStateChanged;

            UploadReplayRequest.StateChangedEvent -= OnUploadRequestStateChanged;
            UserRequest.StateChangedEvent -= OnUserRequestStateChanged;

            PluginConfig.ScoresContextChangedEvent -= OnScoresContextWasChanged;
            LeaderboardState.ScoresScopeChangedEvent -= OnScoresScopeWasSelected;
            LeaderboardState.IsVisibleChangedEvent -= OnIsVisibleChanged;
            LeaderboardEvents.UpButtonWasPressedAction -= OnPreviousPageClick;
            LeaderboardEvents.AroundButtonWasPressedAction -= OnAroundMeClick;
            LeaderboardEvents.DownButtonWasPressedAction -= OnNextPageClick;
            LeaderboardEvents.CaptorClanWasClickedEvent -= OnCaptorClanClick;
            ProfileManager.FriendsUpdatedEvent -= OnFriendsUpdated;
            LeaderboardsCache.CacheWasChangedEvent -= OnCacheUpdated;
        }

        #endregion

        #region SetGlobalBloomShaderParameter

        private static readonly int FakeBloomAmountPropertyID = Shader.PropertyToID("_FakeBloomAmount");

        private static void SetFakeBloomProperty() {
            bool enableFakeBloom;

            try {
                var mainSystemInit = Resources.FindObjectsOfTypeAll<MainSystemInit>()[0];
                enableFakeBloom = mainSystemInit._settingsManager.settings.quality.mainEffect == BeatSaber.Settings.QualitySettings.MainEffectOption.Off;
            } catch (Exception) {
                enableFakeBloom = false;
            }

            Shader.SetGlobalFloat(FakeBloomAmountPropertyID, enableFakeBloom ? 1.0f : 0.0f);
        }

        #endregion

        #region Scores Update

        private bool _updateRequired;

        private void TryUpdateScores() {
            _updateRequired = true;
            if (!LeaderboardState.IsVisible) return;
            UpdateScores();
        }

        private void UpdateScores() {
            _updateRequired = false;

            switch (LeaderboardState.leaderboardType) {
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

        private void LoadPlayerScores() {
            if (!ProfileManager.TryGetUserId(out var userId)) return;
            ScoresRequest.SendPage(_lastSelectedBeatmap, userId, Context, Scope, _lastSelectedPage);
        }

        private void SeekPlayerScores() {
            if (!ProfileManager.TryGetUserId(out var userId)) return;
            ScoresRequest.SendSeek(_lastSelectedBeatmap, userId, Context, Scope);
        }

        private void LoadClanScores() {
            ClanScoresRequest.Send(_lastSelectedBeatmap, _lastSelectedPage);
        }

        #endregion

        #region Request Events

        private LeaderboardKey _uploadLeaderboardKey;

        private void OnUploadRequestStateChanged(IWebRequest<Score> instance, WebRequests.RequestState state, string? failReason) {
            if (!_lastSelectedBeatmap.IsValid()) return;

            switch (state) {
                case WebRequests.RequestState.Started:
                    _uploadLeaderboardKey = LeaderboardKey.FromBeatmap(_lastSelectedBeatmap);
                    break;
                case WebRequests.RequestState.Finished:
                    if (!_uploadLeaderboardKey.Equals(LeaderboardKey.FromBeatmap(_lastSelectedBeatmap))) return;
                    TryUpdateScores();
                    break;
            }
        }

        private void OnScoresRequestStateChanged(IWebRequest<ScoresTableContent> instance, WebRequests.RequestState state, string? failReason) {
            if (state is not WebRequests.RequestState.Finished || LeaderboardState.leaderboardType is not LeaderboardType.SongDiffPlayerScores) return;
            _lastSelectedPage = instance.Result?.CurrentPage ?? 1;
        }

        private void OnUserRequestStateChanged(IWebRequest<User> instance, WebRequests.RequestState state, string? failReason) {
            if (state is not WebRequests.RequestState.Finished) return;
            TryUpdateScores();
        }

        private void OnFriendsUpdated() {
            if (_selectedScoreScope is not ScoresScope.Friends) return;
            TryUpdateScores();
        }

        private void OnCacheUpdated() {
            if (LeaderboardState.leaderboardType is not LeaderboardType.SongDiffClanScores) return;
            if (!LeaderboardsCache.TryGetLeaderboardInfo(LeaderboardState.SelectedLeaderboardKey, out var cacheEntry)) return;
            if (FormatUtils.GetRankedStatus(cacheEntry.DifficultyInfo) is RankedStatus.Ranked) return;
            LeaderboardState.leaderboardType = LeaderboardType.SongDiffPlayerScores;
            TryUpdateScores();
        }

        #endregion

        #region UI Events

        private void OnIsVisibleChanged(bool isVisible) {
            if (!isVisible || !_updateRequired) return;
            UpdateScores();
        }

        public void OnLeaderboardSet(BeatmapKey beatmapKey) {
            var level = _beatmapLevelsModel.GetBeatmapLevel(beatmapKey.levelId);
            if (level == null) return;

            Plugin.Log.Debug($"OnLeaderboardSet: {beatmapKey.levelId}, diff: {beatmapKey.difficulty}");
            _lastSelectedBeatmap = beatmapKey;
            _lastSelectedPage = 1;

            TryUpdateScores();

            LeaderboardState.SelectedBeatmapLevel = level;
            LeaderboardState.SelectedBeatmapKey = beatmapKey;
        }

        private void OnScoresScopeWasSelected(ScoresScope scope) {
            Plugin.Log.Debug($"Attempt to switch score scope from [{_selectedScoreScope}] to [{scope}]");

            if (_selectedScoreScope != scope) {
                LeaderboardState.leaderboardType = LeaderboardType.SongDiffPlayerScores;
                _selectedScoreScope = scope;
                _lastSelectedPage = 1;

                TryUpdateScores();
            }
        }

        private void OnScoresContextWasChanged(int context) {
            Plugin.Log.Debug($"Attempt to switch score context from [{_selectedScoreContext}] to [{context}]");

            if (_selectedScoreContext != context) {
                LeaderboardState.leaderboardType = LeaderboardType.SongDiffPlayerScores;
                _selectedScoreContext = context;
                _lastSelectedPage = 1;

                TryUpdateScores();
            }
        }

        private void OnPreviousPageClick() {
            if (_lastSelectedPage <= 1) {
                _lastSelectedPage = 1;
                return;
            }

            _lastSelectedPage--;
            TryUpdateScores();
        }

        private void OnNextPageClick() {
            _lastSelectedPage++;
            TryUpdateScores();
        }

        private void OnAroundMeClick() {
            SeekPlayerScores();
        }

        private void OnCaptorClanClick() {
            LeaderboardState.leaderboardType = LeaderboardState.leaderboardType switch {
                LeaderboardType.SongDiffPlayerScores => LeaderboardType.SongDiffClanScores,
                _ => LeaderboardType.SongDiffPlayerScores
            };
            _lastSelectedPage = 1;
            TryUpdateScores();
        }

        #endregion
    }
}