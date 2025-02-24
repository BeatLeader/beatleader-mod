using System;
using System.Linq;

namespace BeatLeader.Interop {
    internal static class SongCoreInterop {

        #region TryGetBeatmapRequirements

        public static bool TryGetBeatmapRequirements(IDifficultyBeatmap beatmap, out string[]? requirements) {
            requirements = null;
            try {
                var data = SongCore.Collections.RetrieveDifficultyData(beatmap);
                if (data == null) return false;
                var reqData = data.additionalDifficultyData;
                if (reqData == null) return false;
                requirements = reqData._requirements;
                return true;
            } catch (Exception e) {
                Plugin.Log.Error($"GetRequirements failed: \r\n {e}");
                return false;
            }
        }

        #endregion

        #region ValidateRequirements

        public static bool ValidateRequirements(IDifficultyBeatmap beatmap) {
            return !TryGetBeatmapRequirements(beatmap, out var requirements)
                || (requirements?.All(x => SongCore.Collections.capabilities.Contains(x)) ?? true);
        }

        #endregion
    }
}