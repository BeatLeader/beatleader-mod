using BeatLeader.UI;
using BeatLeader.UI.Reactive;
using Reactive.Components;

namespace BeatLeader {
    internal class BeatLeaderHubMenuButtonsTheme {
        public SerializableColorSet ReplayManagerButtonColors { get; set; } = new();
        public SerializableColorSet BattleRoyaleButtonColors { get; set; } = new();
        public SerializableColorSet SettingsButtonColors { get; set; } = new();
        public SerializableColorSet EditAvatarButtonColors { get; set; } = new();
    }
}