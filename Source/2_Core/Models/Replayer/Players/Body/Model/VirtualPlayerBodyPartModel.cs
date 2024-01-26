namespace BeatLeader.Models {
    public record VirtualPlayerBodyPartModel(
        string Name,
        string Id,
        string? Category,
        bool HasAlphaSupport
    ) : IVirtualPlayerBodyPartModel;
}