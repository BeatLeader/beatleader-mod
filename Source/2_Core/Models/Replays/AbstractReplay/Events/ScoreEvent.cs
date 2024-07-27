namespace BeatLeader.Models.AbstractReplay {
    public readonly struct ScoreEvent {
        public ScoreEvent(int score, float time) {
            this.score = score;
            this.time = time;
        }
        
        public readonly int score;
        public readonly float time;
    }
}