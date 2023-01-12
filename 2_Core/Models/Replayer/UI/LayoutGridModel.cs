namespace BeatLeader.Models {
    internal class LayoutGridModel : ILayoutGridModel {
        public LayoutGridModel(float cellSize, float lineThickness) {
            CellSize = cellSize;
            LineThickness = lineThickness;
        }
        public LayoutGridModel() { }

        public float CellSize { get; set; }
        public float LineThickness { get; set; }
    }
}
