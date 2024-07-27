using JetBrains.Annotations;

namespace BeatLeader.Models.BeatSaver {
    [PublicAPI]
    public class MapDetail {
        public string? id;
        public MapDetailMetadata? metadata;
        public UserDetail? uploader;
        public MapVersion[]? versions;
    }
}