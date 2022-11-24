using BeatLeader.Models;
using System.Collections.Generic;

namespace BeatLeader {
    internal class LayoutMapsConfig : SerializableSingleton<LayoutMapsConfig>, ILayoutMapsSource {
        public IDictionary<string, LayoutMapData> Maps { get; set; } = new Dictionary<string, LayoutMapData>();
    }
}
