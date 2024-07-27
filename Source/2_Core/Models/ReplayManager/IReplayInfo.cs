namespace BeatLeader.Models {
    public interface IReplayInfo {
        string PlayerID { get; }
        string PlayerName { get; }
        
        string SongName { get; }
        string SongDifficulty { get; }
        string SongMode { get; }
        string SongHash { get; }
        LevelEndType LevelEndType { get; }
        
        float FailTime { get; }
        int Score { get; }
        long Timestamp { get; }
    }
}