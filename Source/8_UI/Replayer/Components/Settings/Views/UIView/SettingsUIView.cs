using BeatLeader.Components;
using BeatLeader.UI.BSML_Addons;
using UnityEngine;

namespace BeatLeader.UI.Replayer {
    [BSMLComponent(Namespace = "Replayer")]
    internal class SettingsUIView : ReeUIComponentV3<SettingsUIView>, ReplayerSettingsPanel.ISegmentedControlView {
        public ReplayerSettingsPanel.SettingsView Key => ReplayerSettingsPanel.SettingsView.UIView;
        public (string, Sprite) Value { get; } = ("UI", BundleLoader.UIIcon);

        public void SetActive(bool active) {
            Content.SetActive(active);
        }

        public void Setup(Transform? trans) {
            ContentTransform.SetParent(trans, false);
        }
    }
}