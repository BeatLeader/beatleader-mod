namespace BeatLeader.Models.AbstractReplay {
    public interface IReplayData {
        bool LeftHanded { get; }
        float? FixedHeight { get; }
        float JumpDistance { get; }
        
        float FailTime { get; }
        int Timestamp { get; }
        
        Player? Player { get; }
        PracticeSettings? PracticeSettings { get; }
        GameplayModifiers GameplayModifiers { get; }
    }
}
