namespace BeatLeader.Models {
    internal interface ILayoutGridModel {
        int Height { get; }
        int Width { get; }  
        int CellSize { get; }
        int LineThickness { get; }
    }
}
