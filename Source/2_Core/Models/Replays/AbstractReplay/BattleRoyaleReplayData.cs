using BeatSaber.BeatAvatarSDK;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Models.AbstractReplay {
    [PublicAPI]
    public readonly struct BattleRoyaleReplayData {
        public BattleRoyaleReplayData(AvatarData? avatarData, Color? accentColor) {
            AvatarData = avatarData;
            AccentColor = accentColor;
        }

        public readonly AvatarData? AvatarData;
        public readonly Color? AccentColor;
    }
}