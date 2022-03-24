using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.ScoreProvider
{
    internal class GlobalScoreProvider : IScoreProvider
    {
        private readonly HttpUtils _httpUtils;

        public GlobalScoreProvider(HttpUtils httpUtils)
        {
            _httpUtils = httpUtils;
        }

        public PlatformLeaderboardsModel.ScoresScope getScope()
        {
            return PlatformLeaderboardsModel.ScoresScope.Global;
        }

        public async Task<List<Score>> getScores(IDifficultyBeatmap difficultyBeatmap, int page, CancellationToken scoreRequestToken)
        {
            string hash = difficultyBeatmap.level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
            string diff = difficultyBeatmap.difficulty.ToString();
            string mode = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;

            return await _httpUtils.getData<List<Score>>(
                String.Format(BLConstants.GLOBAL_SCORES_BY_HASH, hash, diff, mode, page, BLConstants.SCORE_PAGE_SIZE),
                scoreRequestToken,
                new());
        }
    }
}
