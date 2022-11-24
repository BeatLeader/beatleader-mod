using UnityEngine;

namespace BeatLeader {
    internal class PlayerViewConfig : SerializableSingleton<PlayerViewConfig> {
        public Models.Vector3 PositionOffset { get; set; } = new Vector3(0, 0, -1);
        public Models.Vector3 RotationOffset { get; set; } = Vector3.zero;
        public int MovementSmoothness { get; set; } = 8;
    }
}
