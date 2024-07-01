using BeatLeader.Components;
using BeatLeader.UI.BSML_Addons;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    [BSMLComponent(Namespace = "Replayer")]
    internal class SettingsOtherView : ReeUIComponentV3<SettingsOtherView>, ReplayerSettingsPanel.ISegmentedControlView {
        public ReplayerSettingsPanel.SettingsView Key => ReplayerSettingsPanel.SettingsView.OtherView;
        public (string, Sprite) Value { get; } = ("Other", BundleLoader.OtherIcon);

        public void SetActive(bool active) {
            Content.SetActive(active);
        }

        public void Setup(Transform? trans) {
            ContentTransform.SetParent(trans, false);
        }
    }
}