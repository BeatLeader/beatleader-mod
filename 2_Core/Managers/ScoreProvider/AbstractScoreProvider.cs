using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.ScoreProvider {
    internal abstract class AbstractScoreProvider : IScoreProvider {

        private readonly HttpUtils _httpUtils;

        public AbstractScoreProvider(HttpUtils httpUtils) {
            _httpUtils = httpUtils;
        }

        public abstract ScoresScope getScope();

        public async Task<Paged<Score>> GetScores(IDifficultyBeatmap difficultyBeatmap, int page, CancellationToken scoreRequestToken) {
            string hash = difficultyBeatmap.level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
            string diff = difficultyBeatmap.difficulty.ToString();
            string mode = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string scope = this.getScope().ToString().ToLowerInvariant();

            string userId = BLContext.profile.id;

            return await _httpUtils.GetPagedData<Score>(
                String.Format(BLConstants.SCORES_BY_HASH_PAGED, hash, diff, mode, scope, HttpUtils.ToHttpParams(new Dictionary<string, object> {
                    { BLConstants.Param.PLAYER, userId },
                    { BLConstants.Param.COUNT, BLConstants.SCORE_PAGE_SIZE },
                    { BLConstants.Param.PAGE, page  }
                })),
                scoreRequestToken,
                null);
        }

        public async Task<Paged<Score>> SeekScores(IDifficultyBeatmap difficultyBeatmap, CancellationToken scoreRequestToken) {
            string hash = difficultyBeatmap.level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
            string diff = difficultyBeatmap.difficulty.ToString();
            string mode = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            string scope = this.getScope().ToString().ToLowerInvariant();

            string userId = BLContext.profile.id;

            return await _httpUtils.GetPagedData<Score>(
                String.Format(BLConstants.SCORES_BY_HASH_SEEK, hash, diff, mode, scope, HttpUtils.ToHttpParams(new Dictionary<string, object> {
                    { BLConstants.Param.PLAYER, userId },
                    { BLConstants.Param.COUNT, BLConstants.SCORE_PAGE_SIZE }
                })),
                scoreRequestToken,
                null);
        }
    }
}
