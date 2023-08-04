using JetBrains.Annotations;

namespace BeatLeader.Models {
    [UsedImplicitly]
    public class SerializableReplayInfo : IReplayInfo {
        public string PlayerID { get; set; }
        public string PlayerName { get; set; }
        public string SongName { get; set; }
        public string SongDifficulty { get; set; }
        public string SongMode { get; set; }
        public string SongHash { get; set; }
        public LevelEndType LevelEndType { get; set; }
        public float FailTime { get; set; }
        public string Timestamp { get; set; }
    }
}