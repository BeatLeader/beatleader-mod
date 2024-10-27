using System.Collections.Generic;
using BeatLeader.Components;
using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public class LayoutEditorSettings {
        public Dictionary<string, LayoutData> ComponentData { get; set; } = new();
    }
}