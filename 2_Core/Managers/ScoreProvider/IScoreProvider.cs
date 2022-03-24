using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using BeatLeader.Models;

namespace BeatLeader.ScoreProvider
{
    internal interface IScoreProvider
    {
        public Task<List<Score>> getScores(IDifficultyBeatmap difficultyBeatmap, int page, CancellationToken scoreRequestToken);

        public PlatformLeaderboardsModel.ScoresScope getScope();

    }
}
