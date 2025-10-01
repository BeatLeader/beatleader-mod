namespace BeatLeader.Models.AbstractReplay {
    public readonly struct HeightEvent {
        public HeightEvent(float time, float height) {
            this.time = time;
            this.height = height;
        }

        public readonly float time;
        public readonly float height;
    }
}