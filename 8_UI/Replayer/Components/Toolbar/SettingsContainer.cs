using BeatLeader.Components.Settings;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using UnityEngine;

namespace BeatLeader.Components {
    internal class SettingsContainer : ReeUIComponentV2 {
        [UIValue("root-content-view")] private RootContentView _contentView;
        [UIValue("navigation-button")] private SubMenuButton _navigationButton;

        protected override void OnInstantiate() {
            _contentView = Instantiate<RootContentView>(transform);
            _navigationButton = Instantiate<SubMenuButton>(transform);
        }
    }
}
