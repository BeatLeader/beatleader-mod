using System;
using System.Collections.Generic;
using System.Threading;
using LeaderboardCore.Interfaces;
using Zenject;

using BeatLeader.Models;
using BeatLeader.ScoreProvider;
using BeatLeader.Manager;

namespace BeatLeader.DataManager
{
    internal class LeaderboardManager : INotifyLeaderboardSet, IInitializable, IDisposable
    {
        private readonly List<IScoreProvider> _scoreProviders;
        private readonly LeaderboardEvents _leaderboardEvents;

        private IScoreProvider? _selectedScoreProvider;
        private int _lastSelectedPage = 1;
        private IDifficultyBeatmap _lastSelectedBeatmap;

        private CancellationTokenSource? scoreRequestToken;

        public LeaderboardManager(List<IScoreProvider> scoreProviders, LeaderboardEvents leaderboardEvents)
        {
            _scoreProviders = scoreProviders;
            _selectedScoreProvider = _scoreProviders.Find(provider => provider.getScope() == ScoresScope.Global);
            _leaderboardEvents = leaderboardEvents;
        }

        #region Initialize/Dispose section

        public void Initialize()
        {
            _leaderboardEvents.GlobalButtonWasPressedAction += SelectGlobalScoreProvider;
            _leaderboardEvents.CountryButtonWasPressedAction += SelectCountryScoreProvider;
            _leaderboardEvents.AroundButtonWasPressedAction += SelectAroundMeScoreProvider;
            //_leaderboardEvents.RefreshButtonWasPressedAction += LoadScores;

            _leaderboardEvents.UpButtonWasPressedAction += FetchPreviousPage;
            _leaderboardEvents.DownButtonWasPressedAction += FetchNextPage;
        }

        public void Dispose()
        {
            _leaderboardEvents.GlobalButtonWasPressedAction -= SelectGlobalScoreProvider;
            _leaderboardEvents.CountryButtonWasPressedAction -= SelectCountryScoreProvider;
            _leaderboardEvents.AroundButtonWasPressedAction -= SelectAroundMeScoreProvider;
            //_leaderboardEvents.RefreshButtonWasPressedAction -= LoadScores;

            _leaderboardEvents.UpButtonWasPressedAction -= FetchPreviousPage;
            _leaderboardEvents.DownButtonWasPressedAction -= FetchNextPage;
        }

        #endregion

        public void OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            Plugin.Log.Debug($"Seleceted beatmap: {difficultyBeatmap.level.songName}, diff: {difficultyBeatmap.difficulty}");
            _lastSelectedBeatmap = difficultyBeatmap;
            _lastSelectedPage = 1;

            LoadScores();
        }

        private async void LoadScores()
        {
            scoreRequestToken?.Cancel();
            scoreRequestToken = new CancellationTokenSource();

            _leaderboardEvents.ScoreRequestStarted();

            Paged<List<Score>> scores = await _selectedScoreProvider.getScores(_lastSelectedBeatmap, _lastSelectedPage, scoreRequestToken.Token);

            _leaderboardEvents.PublishScores(scores);
        }

        #region Select score scope

        public void SelectGlobalScoreProvider()
        {
            ChangeScoreProvider(ScoresScope.Global);
        }

        public void SelectCountryScoreProvider()
        {
            ChangeScoreProvider(ScoresScope.Country);
        }

        public void SelectAroundMeScoreProvider()
        {
            ChangeScoreProvider(ScoresScope.Around_me);
        }

        public void ChangeScoreProvider(ScoresScope scope)
        {
            Plugin.Log.Debug($"Attempt to switch score score from [{_selectedScoreProvider.getScope()}] to [{scope}]");

            IScoreProvider scoreProvider = _scoreProviders.Find(provider => provider.getScope() == scope);
            if (scoreProvider.getScope() != _selectedScoreProvider.getScope())
            {
                _selectedScoreProvider = scoreProvider;
                _lastSelectedPage = 1;

                LoadScores();
            }
        }

        #endregion

        #region Pagination

        private void FetchPreviousPage()
        {
            if (_lastSelectedPage <= 1)
            {
                return;
            }
            _lastSelectedPage--;
            LoadScores();
        }

        private void FetchNextPage()
        {
            _lastSelectedPage++;
            LoadScores();
        }

        #endregion
    }
}
