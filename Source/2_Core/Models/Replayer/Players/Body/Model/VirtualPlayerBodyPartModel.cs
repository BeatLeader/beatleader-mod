namespace BeatLeader.Models {
    public record VirtualPlayerBodyPartModel(
        string Name,
        string Id,
        string? Category,
        BodyNode AnchorNode,
        bool HasAlphaSupport
    ) : IVirtualPlayerBodyPartModel;
}