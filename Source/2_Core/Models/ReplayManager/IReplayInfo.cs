using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public interface IReplayInfo : IReplayHashProvider {
        string PlayerName { get; }
        
        string SongName { get; }
        string SongDifficulty { get; }
        string SongMode { get; }
        string SongHash { get; }
        LevelEndType LevelEndType { get; }
        
        float FailTime { get; }
        int Score { get; }
    }
}