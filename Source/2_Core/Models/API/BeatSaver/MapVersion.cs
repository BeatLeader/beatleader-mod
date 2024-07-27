using JetBrains.Annotations;

namespace BeatLeader.Models.BeatSaver {
    [PublicAPI]
    public class MapVersion {
        public string? hash;
        public string? coverURL;
        public string? downloadURL;
    }
}