using BeatLeader.Themes;

namespace BeatLeader.Models {
    public interface IPlayerProfileSettings {
        string UserMessage { get; }
        ThemeType ThemeType { get; }
        ThemeTier ThemeTier { get; }
        int EffectHue { get; }
        float EffectSaturation { get; }
    }
}