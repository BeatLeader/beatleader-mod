#nullable disable

using JetBrains.Annotations;

namespace BeatLeader.Models {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal class SerializableReplayInfo : IReplayInfo {
        public string PlayerID { get; set; } = null!;
        public string PlayerName { get; set; } = null!;
        public string SongName { get; set; } = null!;
        public string SongDifficulty { get; set; } = null!;
        public string SongMode { get; set; } = null!;
        public string SongHash { get; set; } = null!;
        public LevelEndType LevelEndType { get; set; }
        public float FailTime { get; set; }
        public int Score { get; set; }
        public long Timestamp { get; set; }
    }
}