using BeatLeader.UI;
using BeatLeader.UI.Reactive;
using Reactive.Components;

namespace BeatLeader {
    internal class BeatLeaderHubMenuButtonsTheme {
        public SerializableColorSet ReplayManagerButtonColors { get; set; } = UIStyle.SecondaryButtonColorSet;
        public SerializableColorSet BattleRoyaleButtonColors { get; set; } = UIStyle.SecondaryButtonColorSet;
        public SerializableColorSet SettingsButtonColors { get; set; } = UIStyle.SecondaryButtonColorSet;
        public SerializableColorSet EditAvatarButtonColors { get; set; } = UIStyle.SecondaryButtonColorSet;
    }
}