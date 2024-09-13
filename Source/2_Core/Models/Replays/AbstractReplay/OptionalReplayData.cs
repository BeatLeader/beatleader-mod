using BeatLeader.Models;
using BeatSaber.BeatAvatarSDK;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal record OptionalReplayData(
        AvatarData? AvatarData,
        Color? AccentColor
    ) : IOptionalReplayData;
}