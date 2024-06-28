using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerUISettings {
        public ReplayerFloatingUISettings? FloatingSettings { get; set; }
        public LayoutEditorSettings? LayoutEditorSettings { get; set; }
    }
}