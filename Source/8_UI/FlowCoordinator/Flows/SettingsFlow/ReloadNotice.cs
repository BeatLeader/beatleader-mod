using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReloadNotice : ReactiveComponent {
        #region Setup

        private MenuTransitionsHelper? _transitionsHelper;

        public bool CanBeEnabled => _transitionsHelper != null;

        public void Setup(MenuTransitionsHelper transitionsHelper) {
            _transitionsHelper = transitionsHelper;
        }

        #endregion

        #region Construct

        protected override GameObject Construct() {
            return new BsPrimaryButton {
                    Text = "Reload Now",
                    Skew = UIStyle.Skew,
                    Enabled = false,
                    OnClick = () => _transitionsHelper?.RestartGame()
                }
                .AsFlexItem(size: new() { x = 20f })
                .InNamedRail("These changes will be applied after game reload ")
                .Use();
        }

        #endregion
    }
}