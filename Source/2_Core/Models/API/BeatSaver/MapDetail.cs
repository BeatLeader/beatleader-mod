using JetBrains.Annotations;

namespace BeatLeader.Models.BeatSaver {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal class MapDetail {
        public string? id;
        public MapDetailMetadata? metadata;
        public UserDetail? uploader;
        public MapVersion[]? versions;
    }
}