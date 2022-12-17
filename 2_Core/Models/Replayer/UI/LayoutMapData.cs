namespace BeatLeader.Models {
    internal struct LayoutMapData {
        public SerializableVector2 position;
        public SerializableVector2 anchor;
        public int layer;
        public bool enabled;

        public LayoutMapData CopyWith(
            SerializableVector2? position = null,
            SerializableVector2? anchor = null, 
            int? layer = null,
            bool? enabled = null) {
            this.position = position ?? this.position;
            this.anchor = anchor ?? this.anchor;
            this.layer = layer ?? this.layer;
            this.enabled = enabled ?? this.enabled;
            return this;
        }
    }
}
