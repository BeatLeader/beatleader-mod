using BeatLeader.Components;
using BeatLeader.Models;
using System.Collections.Generic;

namespace BeatLeader {
    internal class LayoutEditorConfig : SerializableSingleton<LayoutEditorConfig>, ILayoutMapsSource {
        public LayoutGridModel LayoutGridModel { get; set; } = new LayoutGridModel(5, 0.4f);
        public IDictionary<string, LayoutMap> LayoutMaps { get; set; } = new Dictionary<string, LayoutMap>();

        public bool TryRequestLayoutMap(EditableElement element, out LayoutMap map) {
            return LayoutMaps.TryGetValue(element.Name, out map);
        }
        public void OverrideLayoutMap(EditableElement element, LayoutMap map) {
            LayoutMaps[element.Name] = map;
        }
    }
}
