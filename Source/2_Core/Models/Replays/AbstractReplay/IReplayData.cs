namespace BeatLeader.Models.AbstractReplay {
    public interface IReplayData {
        bool LeftHanded { get; }
        float? FixedHeight { get; }
        float JumpDistance { get; }
        int Timestamp { get; }

        float FinishTime { get; }
        ReplayFinishType FinishType { get; }

        Player? Player { get; }
        PracticeSettings? PracticeSettings { get; }
        GameplayModifiers GameplayModifiers { get; }
    }
}