using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.ScoreProvider
{
    internal class AroundMeScoreProvider : IScoreProvider
    {
        private readonly HttpUtils _httpUtils;

        public AroundMeScoreProvider(HttpUtils httpUtils)
        {
            _httpUtils = httpUtils;
        }

        public ScoresScope getScope()
        {
            return ScoresScope.Around_me;
        }

        public async Task<Paged<List<Score>>> getScores(IDifficultyBeatmap difficultyBeatmap, int page, CancellationToken scoreRequestToken)
        {
            string hash = difficultyBeatmap.level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
            string diff = difficultyBeatmap.difficulty.ToString();
            string mode = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;

            string userId = BLContext.profile.id;

            return await _httpUtils.GetPagedData<List<Score>>(
                String.Format(BLConstants.AROUNG_ME_SCORES_BY_HASH, hash, diff, mode, userId, page, BLConstants.SCORE_PAGE_SIZE, userId),
                scoreRequestToken,
                new());
        }
    }
}
