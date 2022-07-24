using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader {
    internal static class TableLayoutUtils {
        #region Constants

        private const float TotalWidth = 80.0f;
        private const float Spacing = 1.0f;
        private const float Pad = 2.0f;
        private const int ColumnsCount = 7;

        private const float RankMinWidth = 3.0f;
        private const float ModifiersMinWidth = 2.0f;
        private const float AccMinWidth = 9.0f;
        private const float PPMinWidth = 9.0f;
        private const float ScoreMinWidth = 3.0f;

        private const float ApproxCharacterWidth = 1.2f;
        private const float ModifiersCharacterWidth = 0.8f;

        #endregion

        #region CalculateFlexibleWidths

        public static void CalculateFlexibleWidths(float flexibleWidth, string modifiersString,
            out float nameColumnWidth,
            out float modifiersColumnWidth
        ) {
            modifiersColumnWidth = Mathf.Max(ModifiersMinWidth, ModifiersCharacterWidth * modifiersString.Length);
            nameColumnWidth = flexibleWidth - modifiersColumnWidth;
        }

        #endregion

        #region RecalculateTableLayout //TODO: Make automatic 

        public static void CalculateColumns(Paged<Score> scoresData,
            out float rankColumnWidth,
            out float accColumnWidth,
            out float ppColumnWidth,
            out float scoreColumnWidth,
            out float flexibleWidth,
            out bool hasPP
        ) {
            var maximalRank = 0;
            var maximalScore = 0;
            hasPP = false;

            if (scoresData.selection != null) {
                if (scoresData.selection.pp > 0) hasPP = true;
                if (scoresData.selection.modifiedScore > maximalScore) maximalScore = scoresData.selection.modifiedScore;
                if (scoresData.selection.rank > maximalRank) maximalRank = scoresData.selection.rank;
            }

            foreach (var score in scoresData.data) {
                if (score.pp > 0) hasPP = true;
                if (score.modifiedScore > maximalScore) maximalScore = score.modifiedScore;
                if (score.rank > maximalRank) maximalRank = score.rank;
            }

            rankColumnWidth = CalculateRankWidth(maximalRank);
            accColumnWidth = AccMinWidth;
            ppColumnWidth = PPMinWidth;
            scoreColumnWidth = CalculateScoreWidth(maximalScore);
            flexibleWidth = CalculateFlexibleWidth(rankColumnWidth, accColumnWidth, ppColumnWidth, scoreColumnWidth, hasPP);
        }

        private static float CalculateRankWidth(int maximalRank) {
            var charCount = maximalRank.ToString().Length;
            return Mathf.Max(RankMinWidth, ApproxCharacterWidth * charCount);
        }

        private static float CalculateScoreWidth(int maximalScore) {
            var charCount = maximalScore.ToString("N0", FormatUtils.ScoreFormatInfo).Length;
            return Mathf.Max(ScoreMinWidth, ApproxCharacterWidth * charCount);
        }

        private static float CalculateFlexibleWidth(
            float rankColumnWidth,
            float accColumnWidth,
            float ppColumnWidth,
            float scoreColumnWidth,
            bool hasPP
        ) {
            var result = TotalWidth - Pad * 2;

            if (hasPP) {
                result -= ppColumnWidth;
                result -= Spacing * (ColumnsCount - 1);
            } else {
                result -= Spacing * (ColumnsCount - 2);
            }

            result -= rankColumnWidth;
            result -= scoreColumnWidth;
            result -= accColumnWidth;

            return result;
        }

        #endregion
    }
}