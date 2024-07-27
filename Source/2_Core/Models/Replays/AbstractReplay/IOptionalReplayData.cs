using UnityEngine;

namespace BeatLeader.Models {
    public interface IOptionalReplayData {
        AvatarData? AvatarData { get; }
        Color? AccentColor { get; }
    }
}