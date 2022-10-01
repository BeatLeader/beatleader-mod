using BeatLeader.DataManager;
using BeatLeader.Models;
using System.Linq;
using Zenject;

namespace BeatLeader.Replayer.Tweaking
{
    internal class ModifiersTweak : GameTweak
    {
        [Inject] private readonly ReplayLaunchData _launchData;

        public override void Initialize()
        {
            Plugin.Log.Notice("[Tweaker] Loading modifiers from server...");
            if (!LoadModifiersIfNeeded(_launchData.DifficultyBeatmap))
                Plugin.Log.Error("[Tweaker] Unable to load modifiers from server, scores may be differ!");
        }
        public override void Dispose()
        {
            Plugin.Log.Notice("[Tweaker] Loading modifiers back...");
            ModifiersMapManager.LoadGameplayModifiersMap();
            Plugin.Log.Notice("[Tweaker] Modifiers successfully loaded");
        }

        private bool LoadModifiersIfNeeded(IDifficultyBeatmap beatmap)
        {
            var key = LeaderboardKey.FromBeatmap(beatmap);
            var loadingResult = LeaderboardsCache.TryGetLeaderboardInfo(key, out var data);
            if (!loadingResult) return false;

            var diffInfo = data.DifficultyInfo;
            var modifiersMap = diffInfo.modifierValues;

            var applyPositive = !FormatUtils.NegativeModifiersAppliers.Contains(FormatUtils.GetRankedStatus(diffInfo));
            ModifiersMapManager.LoadCustomModifiersMap(modifiersMap, x => x > 0 && !applyPositive ? 0 : x);

            return true;
        }
    }
}
