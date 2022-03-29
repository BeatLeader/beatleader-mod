using System.Threading;
using System.Threading.Tasks;

using BeatLeader.Models;

namespace BeatLeader.ScoreProvider {
    internal interface IScoreProvider {

        public Task<Paged<Score>> GetScores(IDifficultyBeatmap difficultyBeatmap, int page, CancellationToken scoreRequestToken);

        public Task<Paged<Score>> SeekScores(IDifficultyBeatmap difficultyBeatmap, CancellationToken scoreRequestToken);

        public ScoresScope getScope();

    }
}
