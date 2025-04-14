using HMUI;
using Reactive;
using Reactive.BeatSaber.Components;
using Reactive.Components;
using Reactive.Yoga;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleBattleSetupViewController : ViewController {
        #region Injection

        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;

        #endregion

        #region Setup

        private void Awake() {
            new Layout {
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
                    }.AsFlexItem()
                }
            }.AsFlexGroup(direction: FlexDirection.Column).Use(transform);
        }

        #endregion
    }
}