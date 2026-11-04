using BeatLeader.UI.Reactive.Components;
using Reactive;
using Reactive.BeatSaber;
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
            return new BsPrimaryTextButton {
                    FlexItem = {
                        Size = new() { x = 20f }
                    },

                    Text = "Reload Now",
                    Skew = BeatSaberStyle.Skew,
                    OnClick = () => _transitionsHelper?.RestartGame()
                }
                .InNamedRail("These changes will be applied after game reload ")
                .With(x => x.Enabled = false)
                .Use();
        }

        #endregion
    }
}