namespace BeatLeader.Models.AbstractReplay {
    public class GenericReplayData : IReplayData {
        public GenericReplayData(
            float failTime,
            int timestamp,
            bool leftHanded,
            float jumpDistance,
            float? fixedHeight,
            GameplayModifiers gameplayModifiers,
            Player? player = null,
            PracticeSettings? practiceSettings = null
        ) {
            FailTime = failTime;
            Timestamp = timestamp;
            LeftHanded = leftHanded;
            GameplayModifiers = gameplayModifiers;
            JumpDistance = jumpDistance;
            Player = player;
            PracticeSettings = practiceSettings;
            FixedHeight = fixedHeight;
        }

        public bool LeftHanded { get; }
        public float? FixedHeight { get; }
        public float JumpDistance { get; }

        public float FailTime { get; }
        public int Timestamp { get; }

        public Player? Player { get; }
        public PracticeSettings? PracticeSettings { get; }
        public GameplayModifiers GameplayModifiers { get; }
    }
}