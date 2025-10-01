using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatSaberMarkupLanguage;
using HMUI;
using Reactive;
using Reactive.Components;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BattleRoyaleFlowCoordinator : FlowCoordinator, IBattleRoyaleHost {
        #region AlternativeFlowCoordinator

        /// <summary>
        /// Actual FlowCoordinator which is used in the BeatLeaderMiniScreenSystem.
        /// BattleRoyaleFlowCoordinator behaves just like an adapter between two screen systems
        /// </summary>
        private class AlternativeFlowCoordinator : FlowCoordinator {
            private new FlowCoordinator _parentFlowCoordinator = null!;
            private FlowCoordinator _alternativeParentFlowCoordinator = null!;
            private FlowCoordinator _dummyFlowCoordinator = null!;
            private ViewController _mainViewController = null!;
            private Action? _returnAction;
            private string? _originalTitle;

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
                _mainViewController = mainViewController;
                ProvideInitialViewControllers(mainViewController, leftViewController, rightViewController);
            }

            public void PushReturnAction(string header, Action action) {
                _returnAction = action;
                _originalTitle = title;
                SetTitle(header);
            }

            public void Present() {
                _alternativeParentFlowCoordinator.PresentFlowCoordinator(
                    this,
                    animationDirection: ViewController.AnimationDirection.Vertical
                );
            }

            public void Dismiss() {
                _alternativeParentFlowCoordinator.DismissFlowCoordinator(
                    this,
                    animationDirection: ViewController.AnimationDirection.Vertical
                );
            }

            public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
                if (!firstActivation) return;
                showBackButton = true;
                SetTitle("Battle Royale");
            }

            public override void BackButtonWasPressed(ViewController viewController) {
                if (_returnAction != null) {
                    _returnAction();
                    _returnAction = null;
                    SetTitle(_originalTitle);
                    return;
                }
                if (_mainViewController.isInTransition) return;
                _parentFlowCoordinator.DismissFlowCoordinator(
                    _dummyFlowCoordinator,
                    animationDirection: ViewController.AnimationDirection.Vertical
                );
                Dismiss();
            }
        }

        #endregion

        #region Injection

        [Inject] private readonly BattleRoyaleOpponentsViewController _opponentsViewController = null!;
        [Inject] private readonly BattleRoyaleGreetingsViewController _greetingsViewController = null!;
        [Inject] private readonly BattleRoyaleBattleSetupViewController _battleSetupViewController = null!;
        [Inject] private readonly BeatLeaderMiniScreenSystem _alternativeScreenSystem = null!;
        [Inject] private readonly ReplayerMenuLoader _replayerMenuLoader = null!;

        #endregion

        #region PushReturnAction

        public void PushReturnAction(string header, Action action) {
            _alternativeFlowCoordinator.PushReturnAction(header, action);
        }

        #endregion

        #region Present

        public void PresentFlowCoordinator(FlowCoordinator coordinator) {
            _alternativeFlowCoordinator.PresentFlowCoordinator(coordinator);
        }

        #endregion

        #region Setup

        private AlternativeFlowCoordinator _alternativeFlowCoordinator = null!;
        private ViewController _emptyViewController = null!;

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                _alternativeFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<AlternativeFlowCoordinator>();
                _emptyViewController = BeatSaberUI.CreateViewController<ViewController>();
                ProvideInitialViewControllers(
                    _emptyViewController,
                    topScreenViewController: null
                );
            }
            if (addedToHierarchy) {
                _alternativeFlowCoordinator.Setup(
                    _parentFlowCoordinator,
                    this,
                    _alternativeScreenSystem.FlowCoordinator,
                    _greetingsViewController,
                    _battleSetupViewController,
                    _opponentsViewController
                );
                _alternativeFlowCoordinator.Present();
                HostStateChangedEvent?.Invoke(true);
            }
        }

        public override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling) {
            base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
            if (removedFromHierarchy) {
                HostStateChangedEvent?.Invoke(false);
            }
        }

        #endregion

        #region BattleRoyaleHost

        public IReadOnlyCollection<BattleRoyaleReplay> PendingReplays => _replays.Values;
        public ITableFilter<IReplayHeader> ReplayFilter => _replayFilter;

        public BeatmapLevelWithKey ReplayBeatmap {
            get => _replayFilter.DifficultyBeatmap;
            set {
                _replayFilter.DifficultyBeatmap = value;
                ReplayBeatmapChangedEvent?.Invoke(value);
            }
        }

        public bool CanLaunchBattle { get; private set; }

        public bool CanMutateLobby {
            get => _canMutateLobby;
            set {
                _canMutateLobby = value;
                CanMutateLobbyStateChangedEvent?.Invoke(value);
            }
        }

        public event Action<BattleRoyaleReplay, object>? ReplayAddedEvent;
        public event Action<BattleRoyaleReplay, object>? ReplayRemovedEvent;
        public event Action<BattleRoyaleReplay>? ReplayNavigationRequestedEvent;
        public event Action? ReplayRefreshRequestedEvent;
        public event Action<BeatmapLevelWithKey>? ReplayBeatmapChangedEvent;
        public event Action<bool>? HostStateChangedEvent;
        public event Action<bool>? CanLaunchBattleStateChangedEvent;
        public event Action<bool>? CanMutateLobbyStateChangedEvent;
        public event Action? BattleLaunchStartedEvent;
        public event Action? BattleLaunchFinishedEvent;

        private readonly Dictionary<IReplayHeader, BattleRoyaleReplay> _replays = new();
        private readonly BeatmapReplayFilter _replayFilter = new();
        private bool _canMutateLobby = true;

        public async void LaunchBattle() {
            BattleLaunchStartedEvent?.Invoke();

            await _replayerMenuLoader.StartBattleRoyaleAsync(
                PendingReplays,
                null,
                null,
                CancellationToken.None
            );

            BattleLaunchFinishedEvent?.Invoke();
        }

        public void AddReplay(IReplayHeader header, object caller) {
            if (_replays.ContainsKey(header)) {
                return;
            }

            var replay = new BattleRoyaleReplay(header);

            _replays.Add(header, replay);
            ReplayAddedEvent?.Invoke(replay, caller);

            RecalculateReplayRanks();
            RefreshLaunchState();
        }

        public void RemoveReplay(IReplayHeader header, object caller) {
            if (!_replays.TryGetValue(header, out var replay)) {
                return;
            }

            _replays.Remove(header);
            ReplayRemovedEvent?.Invoke(replay, caller);

            RecalculateReplayRanks();
            RefreshLaunchState();
        }

        public void RemoveAllReplays() {
            foreach (var (header, replay) in _replays.ToArray()) {
                _replays.Remove(header);
                ReplayRemovedEvent?.Invoke(replay, this);
            }

            RecalculateReplayRanks();
            RefreshLaunchState();
        }

        public void NavigateTo(IReplayHeader header) {
            if (!_replays.TryGetValue(header, out var replay)) {
                return;
            }

            ReplayNavigationRequestedEvent?.Invoke(replay);
        }

        #endregion

        #region Other

        private void RecalculateReplayRanks() {
            var rank = 0;
            foreach (var replay in _replays.Values.OrderBy(static x => x.ReplayHeader.ReplayInfo.Score)) {
                replay.ReplayRank = ++rank;
            }
            ReplayRefreshRequestedEvent?.Invoke();
        }

        private void RefreshLaunchState() {
            CanLaunchBattle = _replays.Count > 1;
            CanLaunchBattleStateChangedEvent?.Invoke(CanLaunchBattle);
        }

        #endregion
    }
}