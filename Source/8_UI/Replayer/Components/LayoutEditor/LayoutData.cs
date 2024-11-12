using BeatLeader.Models;

namespace BeatLeader.Components {
    public struct LayoutData {
        public SerializableVector2 position;
        public SerializableVector2 size;
        public int layer;
        public bool visible;
        public LayoutMigrationRules migrationRules;
    }
}