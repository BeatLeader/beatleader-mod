using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerUISettings {
        public bool ShowUIOnPause { get; set; }
        public bool QuickSettingsEnabled { get; set; }
        public TimelineMarkersMask MarkersMask { get; set; }
        public ReplayerFloatingUISettings? FloatingSettings { get; set; }
        public LayoutEditorSettings? LayoutEditorSettings { get; set; }
    }
}