using BeatLeader.DataManager;
using System.Linq;

namespace BeatLeader.Replayer.Tweaking {
    internal class ModifiersTweak : GameTweak {
        public override void Initialize() {
            Plugin.Log.Notice("[ModifiersLoader] Loading modifiers...");

            var loadingResult = LeaderboardsCache.TryGetLeaderboardInfo(
                LeaderboardState.SelectedLeaderboardKey, out var data);
            if (!loadingResult) {
                Plugin.Log.Error("[ModifiersLoader] Failed to load modifiers, scores may be differ!");
                return;
            }

            var diffInfo = data.DifficultyInfo;
            var modifiersMap = diffInfo.modifierValues;

            var applyPositive = !FormatUtils.NegativeModifiersAppliers.Contains(FormatUtils.GetRankedStatus(diffInfo));
            ModifiersMapManager.LoadCustomModifiersMap(modifiersMap, x => x > 0 && !applyPositive ? 0 : x);

            Plugin.Log.Notice("[ModifiersLoader] Modifiers successfully loaded!");
        }

        public override void Dispose() {
            Plugin.Log.Notice("[ModifiersLoader] Loading modifiers back...");
            ModifiersMapManager.LoadGameplayModifiersMap();
            Plugin.Log.Notice("[ModifiersLoader] Modifiers successfully loaded!");
        }
    }
}
