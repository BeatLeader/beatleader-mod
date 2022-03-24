using System;
using System.Collections.Generic;
using System.Threading;
using LeaderboardCore.Interfaces;

using BeatLeader.Models;
using BeatLeader.ScoreProvider;
using BeatLeader.Manager;

namespace BeatLeader.DataManager
{
    internal class LeaderboardManager : INotifyLeaderboardSet
    {
        private readonly IScoreProvider? _selectedScoreProvider;
        private readonly List<IScoreProvider> _scoreProviders;
        private readonly LeaderboardEvents _leaderboardEvents;

        private CancellationTokenSource? scoreRequestToken;

        public LeaderboardManager(List<IScoreProvider> scoreProviders, LeaderboardEvents leaderboardEvents)
        {
            _scoreProviders = scoreProviders;
            _selectedScoreProvider = _scoreProviders.Find(provider => provider.getScope() == PlatformLeaderboardsModel.ScoresScope.Global);
            _leaderboardEvents = leaderboardEvents;
        }

        async void INotifyLeaderboardSet.OnLeaderboardSet(IDifficultyBeatmap difficultyBeatmap)
        {
            scoreRequestToken?.Cancel();
            scoreRequestToken = new CancellationTokenSource();

            _leaderboardEvents.ScoreRequestStarted();

            Plugin.Log.Debug($"Seleceted beatmap: {difficultyBeatmap.level.songName}, diff: {difficultyBeatmap.difficulty}");
            List<Score> scores = await _selectedScoreProvider.getScores(difficultyBeatmap, 1, scoreRequestToken.Token);

            _leaderboardEvents.PublishScores(scores);

        }

        public void changeScoreProvider(PlatformLeaderboardsModel.ScoresScope scope)
        {
            // TODO when UI buttons will be ready
            //_selectedScoreProvider = _scoreProviders.Find(provider => provider.getScope() == scope);
            throw new NotImplementedException();
        }
    }
}
