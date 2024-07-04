using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BeatLeader.Models;
using BeatLeader.Replayer;
using BeatLeader.UI.Reactive.Components;
using BeatLeader.Utils;
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

            protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
                if (!firstActivation) return;
                showBackButton = true;
                SetTitle("Battle Royale");
            }

            protected override void BackButtonWasPressed(ViewController viewController) {
                if (_returnAction != null) {
                    _returnAction();
                    _returnAction = null;
                    SetTitle(_originalTitle);
                    return;
                }
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
        [Inject] private readonly BeatLeaderHubFlowCoordinator _beatLeaderHubFlowCoordinator = null!;
        [Inject] private readonly BeatLeaderMiniScreenSystem _alternativeScreenSystem = null!;
        [Inject] private readonly ReplayerMenuLoader _replayerMenuLoader = null!;
        [Inject] private readonly AvatarPartsModel _avatarPartsModel = null!;

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

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                _alternativeFlowCoordinator = BeatSaberUI.CreateFlowCoordinator<AlternativeFlowCoordinator>();
                _alternativeFlowCoordinator.Setup(
                    _beatLeaderHubFlowCoordinator,
                    this,
                    _alternativeScreenSystem.FlowCoordinator,
                    _greetingsViewController,
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

        public IReadOnlyCollection<IBattleRoyaleReplay> PendingReplays => _replays.Values;
        public ITableFilter<IReplayHeaderBase> ReplayFilter => _replayFilter;

        public IDifficultyBeatmap? ReplayBeatmap {
            get => _replayFilter.DifficultyBeatmap;
            set {
                _replayFilter.DifficultyBeatmap = value;
                ReplayBeatmapChangedEvent?.Invoke(value);
            }
        }

        public bool CanLaunchBattle { get; private set; }

        public event Action<IBattleRoyaleReplay, object>? ReplayAddedEvent;
        public event Action<IBattleRoyaleReplay, object>? ReplayRemovedEvent;
        public event Action<IBattleRoyaleReplay>? ReplayNavigationRequestedEvent;
        public event Action? ReplayRefreshRequestedEvent;
        public event Action<IDifficultyBeatmap?>? ReplayBeatmapChangedEvent;
        public event Action<bool>? HostStateChangedEvent;
        public event Action<bool>? CanLaunchBattleStateChangedEvent;
        public event Action? BattleLaunchStartedEvent;
        public event Action? BattleLaunchFinishedEvent;

        private readonly Dictionary<IReplayHeaderBase, BattleRoyaleReplay> _replays = new();
        private readonly BeatmapReplayFilter _replayFilter = new();

        public async void LaunchBattle() {
            BattleLaunchStartedEvent?.Invoke();
            await _replayerMenuLoader.StartBattleRoyaleAsync(
                PendingReplays,
                null,
                CancellationToken.None
            );
            BattleLaunchFinishedEvent?.Invoke();
        }

        public void AddReplay(IReplayHeaderBase header, object caller) {
            if (_replays.ContainsKey(header)) return;
            //
            var replayData = CreateReplayData(header);
            var replay = new BattleRoyaleReplay(header, replayData);
            //
            _replays.Add(header, replay);
            ReplayAddedEvent?.Invoke(replay, caller);
            RecalculateReplayRanks();
            RefreshLaunchState();
        }

        public void RemoveReplay(IReplayHeaderBase header, object caller) {
            if (!_replays.TryGetValue(header, out var replay)) return;
            _replays.Remove(header);
            ReplayRemovedEvent?.Invoke(replay, caller);
            RecalculateReplayRanks();
            RefreshLaunchState();
        }

        public void NavigateTo(IReplayHeaderBase header) {
            if (!_replays.TryGetValue(header, out var replay)) return;
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

        private BattleRoyaleOptionalReplayData CreateReplayData(IReplayHeaderBase header) {
            //color
            var replayInfo = header.ReplayInfo;
            var colorSeed = $"{replayInfo.Timestamp}{replayInfo.PlayerID}{replayInfo.SongName}".GetHashCode();
            var color = ColorUtils.RandomColor(rand: new(colorSeed));
            //avatar
            var avatarData = new AvatarData();
            AvatarUtils.RandomizeAvatarByPlayerId(header.ReplayInfo.PlayerID, avatarData, _avatarPartsModel);
            //
            return new BattleRoyaleOptionalReplayData(avatarData, color);
        }

        #endregion
    }
}