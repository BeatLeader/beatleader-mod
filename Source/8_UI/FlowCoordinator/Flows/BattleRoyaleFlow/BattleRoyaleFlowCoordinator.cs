using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Replayer;
using BeatLeader.UI.Hub.Models;
using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleFlowCoordinator : FlowCoordinator, IBattleRoyaleHost {
        #region AlternativeFlowCoordinator

        /// <summary>
        /// Actual FlowCoordinator which is used in the BeatLeaderMiniScreenSystem.
        /// BattleRoyaleFlowCoordinator behaves just like an adapter between two screen systems
        /// </summary>
        private class AlternativeFlowCoordinator : FlowCoordinator {
            private FlowCoordinator _parentFlowCoordinator = null!;
            private FlowCoordinator _alternativeParentFlowCoordinator = null!;
            private FlowCoordinator _dummyFlowCoordinator = null!;

            public void Setup(
                FlowCoordinator parentFlowCoordinator,
                FlowCoordinator dummyFlowCoordinator,
                FlowCoordinator alternativeParentFlowCoordinator,
                ViewController mainViewController,
                ViewController? leftViewController,
                ViewController? rightViewController
            ) {
                _parentFlowCoordinator = parentFlowCoordinator;
                _alternativeParentFlowCoordinator = alternativeParentFlowCoordinator;
                _dummyFlowCoordinator = dummyFlowCoordinator;
                ProvideInitialViewControllers(mainViewController, leftViewController, rightViewController);
            }

            public void Present() {
                _alternativeParentFlowCoordinator.PresentFlowCoordinator(
                    this, animationDirection: ViewController.AnimationDirection.Vertical
                );
            }

            public void Dismiss() {
                _alternativeParentFlowCoordinator.DismissFlowCoordinator(
                    this, animationDirection: ViewController.AnimationDirection.Vertical
                );
            }

            protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
                if (!firstActivation) return;
                showBackButton = true;
                SetTitle("Battle Royale");
            }

            protected override void BackButtonWasPressed(ViewController viewController) {
                _parentFlowCoordinator.DismissFlowCoordinator(
                    _dummyFlowCoordinator, animationDirection: ViewController.AnimationDirection.Vertical
                );
                Dismiss();
            }
        }

        #endregion

        #region Injection

        [Inject] private readonly BattleRoyaleOpponentsViewController _opponentsViewController = null!;
        [Inject] private readonly BattleRoyaleReplaySelectionViewController _replaySelectionViewController = null!;
        [Inject] private readonly BattleRoyaleBattleSetupViewController _battleSetupViewController = null!;
        [Inject] private readonly BeatLeaderHubFlowCoordinator _beatLeaderHubFlowCoordinator = null!;
        [Inject] private readonly BeatLeaderMiniScreenSystem _alternativeScreenSystem = null!;
        [Inject] private readonly ReplayerMenuLoader _replayerMenuLoader = null!;

        #endregion

        #region Setup

        private AlternativeFlowCoordinator _alternativeFlowCoordinator = null!;
        private ViewController _emptyViewController = null!;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                _alternativeFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<AlternativeFlowCoordinator>();
                _alternativeFlowCoordinator.Setup(
                    _beatLeaderHubFlowCoordinator,
                    this,
                    _alternativeScreenSystem.FlowCoordinator,
                    _replaySelectionViewController,
                    _battleSetupViewController,
                    _opponentsViewController
                );
                _emptyViewController = BeatSaberUI.CreateViewController<ViewController>();
                ProvideInitialViewControllers(
                    _emptyViewController,
                    topScreenViewController: null
                );
            }
            if (addedToHierarchy) {
                _alternativeFlowCoordinator.Present();
                HostStateChangedEvent?.Invoke(true);
            }
        }

        protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            HostStateChangedEvent?.Invoke(false);
        }

        #endregion

        #region BattleRoyaleHost

        public IReadOnlyCollection<IReplayHeaderBase> PendingReplays => _replays;
        public IReplayFilter ReplayFilter => _replayFilter;

        public IBeatmapReplayFilterData? FilterData {
            get => _replayFilter.BeatmapFilterData;
            set => _replayFilter.BeatmapFilterData = value;
        }

        public bool CanLaunchBattle { get; private set; }

        public event Action<IReplayHeaderBase, object>? ReplayAddedEvent;
        public event Action<IReplayHeaderBase, object>? ReplayRemovedEvent;
        public event Action<IReplayHeaderBase>? ReplayNavigationRequestedEvent;
        public event Action<bool>? HostStateChangedEvent;
        public event Action<bool>? CanLaunchBattleStateChangedEvent;
        public event Action? BattleLaunchStartedEvent;
        public event Action? BattleLaunchFinishedEvent;

        private readonly HashSet<IReplayHeaderBase> _replays = new();

        private readonly BeatmapReplayFilter _replayFilter = new() {
            filterOffAllWhenDataNotFull = true
        };

        public async void LaunchBattle() {
            BattleLaunchStartedEvent?.Invoke();
            var replays = new Dictionary<Replay, IPlayer?>();
            foreach (var header in _replays) {
                var replay = await header.LoadReplayAsync(default);
                var player = await header.LoadPlayerAsync(false, default);
                replays.Add(replay!, player);
            }
            await _replayerMenuLoader.StartReplaysAsync(replays, null, default);
            BattleLaunchFinishedEvent?.Invoke();
        }

        public void AddReplay(IReplayHeaderBase header, object caller) {
            if (!_replays.Add(header)) return;
            ReplayAddedEvent?.Invoke(header, caller);
            RefreshLaunchState();
        }

        public void RemoveReplay(IReplayHeaderBase header, object caller) {
            if (!_replays.Remove(header)) return;
            ReplayRemovedEvent?.Invoke(header, caller);
            RefreshLaunchState();
        }

        public void NavigateTo(IReplayHeaderBase header) {
            if (!_replays.Contains(header)) return;
            ReplayNavigationRequestedEvent?.Invoke(header);
        }

        private void RefreshLaunchState() {
            CanLaunchBattle = _replays.Count > 1;
            CanLaunchBattleStateChangedEvent?.Invoke(CanLaunchBattle);
        }

        #endregion
    }
}