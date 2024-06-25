using BeatLeader.UI.Reactive;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.UI.Reactive.Yoga;
using HMUI;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleOpponentsViewController : ViewController {
        #region Injection

        [Inject] private readonly IBattleRoyaleHost _battleRoyaleHost = null!;

        #endregion

        #region Construct

        private BattleRoyaleOpponentsList _opponentsList = null!;
        private CanvasGroup _localCanvasGroup = null!;

        private void Awake() {
            new Dummy {
                Children = {
                    //header
                    new BattleRoyaleViewHeader {
                        Text = "Battle Opponents"
                    },
                    //list
                    new BattleRoyaleOpponentsList()
                        .AsFlexItem(grow: 1f, margin: new() { top = 10f })
                        .Bind(ref _opponentsList)
                }
            }.AsFlexGroup(direction: FlexDirection.Column).WithNativeComponent(out _localCanvasGroup).Use(transform);

            _battleRoyaleHost.ReplayAddedEvent += HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent += HandleReplayRemoved;
            _opponentsList.Setup(_battleRoyaleHost);

            _alphaAnimator.SetProgress(0f);
            _alphaAnimator.SetTarget(0f);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _opponentsList.Setup(null);
            _battleRoyaleHost.ReplayAddedEvent -= HandleReplayAdded;
            _battleRoyaleHost.ReplayRemovedEvent -= HandleReplayRemoved;
        }

        #endregion

        #region Animation

        private readonly ValueAnimator _alphaAnimator = new() { LerpCoefficient = 20f };
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
            _alphaAnimator.Push();
            _isViewPresented = true;
        }

        private void DismissView() {
            _alphaAnimator.Pull();
            _isViewPresented = false;
        }

        private void Update() {
            _alphaAnimator.Update();
            _localCanvasGroup.alpha = _alphaAnimator.Progress;
        }

        #endregion

        #region Callbacks

        private void HandleReplayAdded(IBattleRoyaleReplay replay, object caller) {
            _replaysCount++;
            RefreshVisibility();
        }

        private void HandleReplayRemoved(IBattleRoyaleReplay replay, object caller) {
            _replaysCount--;
            RefreshVisibility();
        }

        #endregion
    }
}