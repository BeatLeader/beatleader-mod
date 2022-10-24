using UnityEngine;

namespace BeatLeader {
    [SerializeAutomatically("PlayerViewConfig")]
    internal static class PlayerViewConfig {
        [SerializeAutomatically] public static Models.Vector3 PositionOffset { get; set; } = new Vector3(0, 0, -1);
        [SerializeAutomatically] public static Models.Vector3 RotationOffset { get; set; } = Vector3.zero;
        [SerializeAutomatically] public static int MovementSmoothness { get; set; } = 8;
    }
}
