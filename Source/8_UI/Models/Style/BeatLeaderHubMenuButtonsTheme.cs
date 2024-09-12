using BeatLeader.UI;
using BeatLeader.UI.Reactive;
using Reactive.Components;

namespace BeatLeader {
    internal class BeatLeaderHubMenuButtonsTheme {
        //TODO: make serializable
        public SimpleColorSet ReplayManagerButtonColors { get; set; } = UIStyle.SecondaryButtonColorSet;
        public SimpleColorSet BattleRoyaleButtonColors { get; set; } = UIStyle.SecondaryButtonColorSet;
        public SimpleColorSet SettingsButtonColors { get; set; } = UIStyle.SecondaryButtonColorSet;
        public SimpleColorSet EditAvatarButtonColors { get; set; } = UIStyle.SecondaryButtonColorSet;
    }
}