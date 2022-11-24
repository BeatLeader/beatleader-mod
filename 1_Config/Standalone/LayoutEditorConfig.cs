namespace BeatLeader {
    internal class LayoutEditorConfig : SerializableSingleton<LayoutEditorConfig> {
        public int LineThickness { get; set; } = 1;
        public int CellSize { get; set; } = 7;
    }
}
