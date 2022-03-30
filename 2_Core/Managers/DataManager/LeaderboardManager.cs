using System;
using System.Collections.Generic;
using System.Threading;
using LeaderboardCore.Interfaces;
using Zenject;

using BeatLeader.Models;
using BeatLeader.ScoreProvider;
using BeatLeader.Manager;

namespace BeatLeader.DataManager {
    internal class LeaderboardManager : INotifyLeaderboardSet, IInitializable, IDisposable {
        private readonly List<IScoreProvider> _scoreProviders;

        private IScoreProvider _selectedScoreProvider;
        private int _lastSelectedPage = 1;
        private IDifficultyBeatmap _lastSelectedBeatmap;

        private CancellationTokenSource? scoreRequestToken;

        public LeaderboardManager(List<IScoreProvider> scoreProviders) {
            _scoreProviders = scoreProviders;
            _selectedScoreProvider = _scoreProviders.Find(provider => provider.getScope() == ScoresScope.Global);
        }

        #region Initialize/Dispose section

        public void Initialize() {
            LeaderboardEvents.UploadSuccessAction += LoadScores;

            LeaderboardEvents.ScopeWasSelectedAction += ChangeScoreProvider;
            //LeaderboardEvents.RefreshButtonWasPressedAction += LoadScores;

            LeaderboardEvents.UpButtonWasPressedAction += FetchPreviousPage;
            LeaderboardEvents.AroundButtonWasPressedAction += SeekAroundMePage;
            LeaderboardEvents.DownButtonWasPressedAction += FetchNextPage;
        }

        public void Dispose() {
            LeaderboardEvents.UploadSuccessAction -= LoadScores;

            LeaderboardEvents.ScopeWasSelectedAction -= ChangeScoreProvider;
            //_leaderboardEvents.RefreshButtonWasPressedAction -= LoadScores;

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

        private async void LoadScores() {
            scoreRequestToken?.Cancel();
            scoreRequestToken = new CancellationTokenSource();

            LeaderboardEvents.ScoreRequestStarted();

            Paged<Score> scores = await _selectedScoreProvider.GetScores(_lastSelectedBeatmap, _lastSelectedPage, scoreRequestToken.Token);

            if (scores == null) {
                LeaderboardEvents.NotifyScoresFetchFailed();
            } else {
                LeaderboardEvents.PublishScores(scores);
            }
        }

        private async void SeekScores() {
            scoreRequestToken?.Cancel();
            scoreRequestToken = new CancellationTokenSource();

            LeaderboardEvents.ScoreRequestStarted();

            Paged<Score> scores = await _selectedScoreProvider.SeekScores(_lastSelectedBeatmap, scoreRequestToken.Token);

            if (scores == null) {
                LeaderboardEvents.NotifyScoresFetchFailed();
            } else {
                _lastSelectedPage = scores.metadata.page;
                LeaderboardEvents.PublishScores(scores);
            }
        }

        #endregion

        #region Select score scope

        private void ChangeScoreProvider(ScoresScope scope) {
            Plugin.Log.Debug($"Attempt to switch score score from [{_selectedScoreProvider.getScope()}] to [{scope}]");

            IScoreProvider scoreProvider = _scoreProviders.Find(provider => provider.getScope() == scope);
            if (scoreProvider.getScope() != _selectedScoreProvider.getScope()) {
                _selectedScoreProvider = scoreProvider;
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
