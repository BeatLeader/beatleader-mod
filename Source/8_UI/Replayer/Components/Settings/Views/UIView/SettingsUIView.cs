using BeatLeader.Components;
using BeatLeader.UI.BSML_Addons;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using UnityEngine;
using Dummy = BeatLeader.UI.Reactive.Components.Dummy;

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

        #region Setup

        private ILayoutEditor? _layoutEditor;

        public void Setup(ILayoutEditor layoutEditor) {
            _layoutEditor = layoutEditor;
        }

        #endregion

        #region Construct

        protected override GameObject Construct(Transform parent) {
            return new Dummy {
                Children = {
                    new Reactive.Components.ImageButton {
                        Colors = null,
                        GrowOnHover = true,
                        Image = {
                            Sprite = BundleLoader.EditLayoutIcon
                        }
                    }.AsRectItem(
                        sizeDelta: Vector2.one * 20f
                    ).WithClickListener(HandleLayoutEditorButtonClicked)
                }
            }.WithRectExpand().Use(parent);
        }

        #endregion

        #region Callbacks

        private void HandleLayoutEditorButtonClicked() {
            _layoutEditor!.SetEditorActive(true);
        }

        #endregion
    }
}