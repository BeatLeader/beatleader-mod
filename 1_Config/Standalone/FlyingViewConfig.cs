namespace BeatLeader {
    internal class FlyingViewConfig : SerializableSingleton<FlyingViewConfig> {
        public int FlySpeed { get; set; } = 4;
        public bool DisableOnUnCur { get; set; } = true;
        public float SensitivityX { get; set; } = 0.5f;
        public float SensitivityY { get; set; } = 0.5f;
    }
}
