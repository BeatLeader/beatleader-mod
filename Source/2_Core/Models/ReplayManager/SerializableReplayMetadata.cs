using System.Collections.Generic;

#nullable disable

namespace BeatLeader.Models {
    internal class SerializableReplayMetadata {
        public HashSet<string> Tags { get; set; } = new();
    }
}