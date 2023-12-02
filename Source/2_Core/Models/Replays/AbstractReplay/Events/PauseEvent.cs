namespace BeatLeader.Models.AbstractReplay {
    public readonly struct PauseEvent {
        public PauseEvent(float time, long duration) {
            this.time = time;
            this.duration = duration;
        }
        
        public readonly float time;
        public readonly long duration;
    }
}
