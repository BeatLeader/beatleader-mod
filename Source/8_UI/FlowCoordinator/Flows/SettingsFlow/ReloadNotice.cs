using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using UnityEngine;

namespace BeatLeader.UI.Hub {
    internal class ReloadNotice : ReactiveComponent {
        #region Setup

        private MenuTransitionsHelper? _transitionsHelper;

        public void Setup(MenuTransitionsHelper transitionsHelper) {
            _transitionsHelper = transitionsHelper;
        }

        #endregion

        #region Construct

        protected override GameObject Construct() {
            return new BsPrimaryButton {
                    Skew = UIStyle.Skew
                }
                .WithClickListener(() => _transitionsHelper?.RestartGame())
                .AsFlexItem(size: new() { x = 20f })
                .WithLabel("Reload Now")
                .InNamedRail("These changes will be applied after game reload ")
                .Use();
        }

        #endregion
    }
}