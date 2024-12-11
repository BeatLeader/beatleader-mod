#nullable disable

namespace BeatLeader.Models {
    internal class ChristmasTreeSettings {
        public SerializablePose gameTreePose;
        public SerializablePose webTreePose;
        public SerializableVector3 scale;
        public ChristmasTreeOrnamentSettings[] ornaments;
    }
}