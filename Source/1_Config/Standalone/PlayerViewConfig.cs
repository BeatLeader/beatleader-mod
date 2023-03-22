using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader {
    internal class PlayerViewConfig : SerializableSingleton<PlayerViewConfig> {
        public SerializableVector3 PositionOffset { get; set; } = new(0, 0, -1);
        public SerializableVector3 RotationOffset { get; set; } = Vector3.zero;
        public int MovementSmoothness { get; set; } = 8;
    }
}
