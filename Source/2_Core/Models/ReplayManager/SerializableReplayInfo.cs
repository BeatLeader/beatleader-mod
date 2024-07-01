using JetBrains.Annotations;

#nullable disable

namespace BeatLeader.Models {
    [UsedImplicitly]
    internal class SerializableReplayInfo : IReplayInfo {
        public string PlayerID { get; set; }
        public string PlayerName { get; set; }
        public string SongName { get; set; }
        public string SongDifficulty { get; set; }
        public string SongMode { get; set; }
        public string SongHash { get; set; }
        public LevelEndType LevelEndType { get; set; }
        public float FailTime { get; set; }
        public int Score { get; set; }
        public long Timestamp { get; set; }
    }
}