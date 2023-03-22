using UnityEngine;
using Vector3 = BeatLeader.Models.Replay.Vector3;

namespace BeatLeader {
    internal class PlayerViewConfig : SerializableSingleton<PlayerViewConfig> {
        public Vector3 PositionOffset { get; set; } = new UnityEngine.Vector3(0, 0, -1);
        public Vector3 RotationOffset { get; set; } = UnityEngine.Vector3.zero;
        public int MovementSmoothness { get; set; } = 8;
    }
}
