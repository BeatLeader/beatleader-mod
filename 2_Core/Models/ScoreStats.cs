namespace BeatLeader.Models {
    internal class ScoreStats {
        public int id;
        public int scoreId;

        public AccuracyTracker accuracyTracker;
        public HitTracker hitTracker;
        public WinTracker winTracker;
        public ScoreGraphTracker scoreGraphTracker;
    }

    internal class AccuracyTracker {
        public int id;

        public float[] gridAcc;
        public float accLeft;
        public float accRight;

        public float[] leftAverageCut;
        public float leftPreswing;
        public float leftPostswing;
        public float leftTimeDependence;

        public float[] rightAverageCut;
        public float rightPreswing;
        public float rightPostswing;
        public float rightTimeDependence;
    }

    internal class HitTracker {
        public int id;
        public int maxCombo;

        public int leftBadCuts;
        public int leftBombs;
        public int leftMiss;

        public int rightBadCuts;
        public int rightBombs;
        public int rightMiss;
    }

    internal class WinTracker {
        public int id;

        public float jumpDistance;
        public int nbOfPause;
        public int totalScore;
        public float endTime;
        public bool won;
    }

    internal class ScoreGraphTracker {
        public float[] graph;
    }
}
