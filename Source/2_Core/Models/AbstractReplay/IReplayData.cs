namespace BeatLeader.Models.AbstractReplay {
    public interface IReplayData {
        bool LeftHanded { get; }
        float JumpDistance { get; }
        float FailTime { get; }
        string Timestamp { get; }
        Player? Player { get; }
        PracticeSettings? PracticeSettings { get; }
        GameplayModifiers GameplayModifiers { get; }
    }
}
