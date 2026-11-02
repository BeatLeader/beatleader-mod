using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleBattleSetupViewController : ViewController {
        #region Setup

        private void Awake() {
            new Layout {
                FlexController = {
                    FlexDirection = FlexDirection.Column,
                    JustifyContent = Justify.SpaceAround
                },

                Children = {
                    //header
                    new BattleRoyaleViewHeader {
                        Text = "Battle Setup"
                    },
                    //info
                    new Label {
                        Text = "Unfortunately Monke didn't know what to put in this section. " +
                            "If you think that you do, you can tell us about it on our Discord!",
                        EnableWrapping = true
                    }
                }
            }.Use(transform);
        }

        #endregion
    }
}