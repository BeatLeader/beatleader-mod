using JetBrains.Annotations;

namespace BeatLeader.Models.BeatSaver {
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    internal class MapVersion {
        public string? hash;
        public string? coverURL;
        public string? downloadURL;
    }
}