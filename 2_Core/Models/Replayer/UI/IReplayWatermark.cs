namespace BeatLeader.Models {
    internal interface IReplayWatermark {
        bool Enabled { get; set; }
        bool CanBeDisabled { get; }
    }
}
