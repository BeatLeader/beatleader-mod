namespace BeatLeader.Models.AbstractReplay {
    public class GenericReplayData : IReplayData {
        public GenericReplayData(
            float failTime, string timestamp, bool leftHanded,
            GameplayModifiers gameplayModifiers, float jumpDistance, 
            Player? player = null, PracticeSettings? practiceSettings = null) {
            FailTime = failTime;
            Timestamp = timestamp;
            LeftHanded = leftHanded;
            GameplayModifiers = gameplayModifiers;
            JumpDistance = jumpDistance;
            Player = player;
            PracticeSettings = practiceSettings;
        }

        public float JumpDistance { get; }
        public float FailTime { get; }
        public string Timestamp { get; }
        public bool LeftHanded { get; }
        public Player? Player { get; }
        public PracticeSettings? PracticeSettings { get; }
        public GameplayModifiers GameplayModifiers { get; }
    }
}