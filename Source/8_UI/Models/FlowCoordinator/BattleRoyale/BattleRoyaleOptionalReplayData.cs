using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal record BattleRoyaleOptionalReplayData(
        AvatarData? AvatarData,
        Color? AccentColor
    ) : IOptionalReplayData;
}