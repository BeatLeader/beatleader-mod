using BeatLeader.UI.Reactive.Components;
using HMUI;
using Reactive;
using Reactive.Components;
using Reactive.Yoga;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleOpponentsViewController : ViewController {
        #region Injection

        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;

        #endregion

        #region Construct

        private BattleRoyaleOpponentsList _opponentsList = null!;
        private AnimatedState<float> _alpha = null!;

        private void Awake() {
            var layout = new Layout {
                Children = {
                    //header
                    new BattleRoyaleViewHeader {
                        Text = "Battle Opponents"
                    },
                    //list
                    new BattleRoyaleOpponentsList()
                        .AsFlexItem(flexGrow: 1f, margin: new() { top = 10f })
                        .Bind(ref _opponentsList)
                }
            }.AsFlexGroup(direction: FlexDirection.Column);

            _alpha = StateUtils.RememberAnimatedFloat(layout, 0f, 20.fact);
            layout.WithAlpha(_alpha).Use(transform);

            _battleRoyaleHost.ReplayAddedEvent += HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent += HandleReplayRemoved;
            _opponentsList.Setup(_battleRoyaleHost);
        }

        public override void OnDestroy() {
            base.OnDestroy();
            _opponentsList.Setup(null);
            _battleRoyaleHost.ReplayAddedEvent -= HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent -= HandleReplayRemoved;
        }

        #endregion

        #region Animation

        private int _replaysCount;
        private bool _isViewPresented;

        private void RefreshVisibility() {
            if (_replaysCount > 0 && !_isViewPresented) {
                PresentView();
            } else if (_replaysCount == 0 && _isViewPresented) {
                DismissView();
            }
        }

        private void PresentView() {
            _alpha.TargetValue = 1f;
            _isViewPresented = true;
        }

        private void DismissView() {
            _alpha.TargetValue = 0f;
            _isViewPresented = false;
        }

        #endregion

        #region Callbacks

        private void HandleReplayAdded(BattleRoyaleReplay replay, object caller) {
            _replaysCount++;
            RefreshVisibility();
        }

        private void HandleReplayRemoved(BattleRoyaleReplay replay, object caller) {
            _replaysCount--;
            RefreshVisibility();
        }

        #endregion
    }
}