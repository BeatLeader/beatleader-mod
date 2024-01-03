namespace BeatLeader.Models {
    public record VirtualPlayerConfig(
        VirtualPlayerBodyConfig BodyConfig,
        VirtualPlayerSabersConfig SabersConfig
    );
}