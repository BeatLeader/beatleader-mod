namespace BeatLeader.Models.AbstractReplay {
    public readonly struct PlayerMovementFrame {
        public PlayerMovementFrame(
            float time, 
            SerializablePose headPose, 
            SerializablePose leftHandPose, 
            SerializablePose rightHandPose) {
            this.time = time;
            this.headPose = headPose;
            this.leftHandPose = leftHandPose;
            this.rightHandPose = rightHandPose;
        }

        public readonly float time;
        public readonly SerializablePose headPose;
        public readonly SerializablePose leftHandPose;
        public readonly SerializablePose rightHandPose;
    }
}