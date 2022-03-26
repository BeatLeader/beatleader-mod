using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using BeatLeader.Models;

namespace BeatLeader.ScoreProvider
{
    internal interface IScoreProvider
    {
        public Task<Paged<List<Score>>> getScores(IDifficultyBeatmap difficultyBeatmap, int page, CancellationToken scoreRequestToken);

        public ScoresScope getScope();

    }
}
