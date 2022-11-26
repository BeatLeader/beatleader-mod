using BeatLeader.Models;
using System.Collections.Generic;

namespace BeatLeader {
    internal class LayoutEditorConfig : SerializableSingleton<LayoutEditorConfig>, ILayoutMapsSource {
        public IDictionary<string, LayoutMapData> Maps { get; set; } = new Dictionary<string, LayoutMapData>();

        public int LineThickness { get; set; } = 1;
        public int CellSize { get; set; } = 7;
    }
}
