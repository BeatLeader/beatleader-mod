namespace BeatLeader.Models {
    public interface IVirtualPlayerBodyPartModel {
        bool HasAlphaSupport { get; }
        BodyNode AnchorNode { get; }
        
        string? Category { get; }
        string Name { get; }
        string Id { get; }
    }
}