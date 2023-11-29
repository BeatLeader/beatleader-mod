namespace BeatLeader.Models.AbstractReplay {
    public record GenericReplayData(
        float FinishTime,
        ReplayFinishType FinishType,
        int Timestamp,
        bool LeftHanded,
        float JumpDistance,
        float? FixedHeight,
        GameplayModifiers GameplayModifiers,
        Player? Player = null,
        PracticeSettings? PracticeSettings = null
    ) : IReplayData;
}