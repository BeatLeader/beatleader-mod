namespace BeatLeader.UI.Replayer {
    internal interface IReplayWatermark {
        bool Enabled { get; set; }
        bool CanBeDisabled { get; }
    }
}
