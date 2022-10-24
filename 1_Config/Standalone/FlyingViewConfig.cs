namespace BeatLeader {
    [SerializeAutomatically("FlyingViewConfig")]
    internal static class FlyingViewConfig {
        [SerializeAutomatically] public static int FlySpeed { get; set; } = 4;
        [SerializeAutomatically] public static bool DisableOnUnCur { get; set; } = true;
        [SerializeAutomatically] public static float SensitivityX { get; set; } = 0.5f;
        [SerializeAutomatically] public static float SensitivityY { get; set; } = 0.5f;
    }
}
