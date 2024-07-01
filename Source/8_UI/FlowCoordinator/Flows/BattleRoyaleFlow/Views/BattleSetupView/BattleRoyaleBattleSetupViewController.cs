using BeatLeader.UI.Hub.Models;
using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using HMUI;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleBattleSetupViewController : ViewController {
        #region Injection

        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;

        #endregion

        #region Setup

        private void Awake() {
            new Dummy {
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