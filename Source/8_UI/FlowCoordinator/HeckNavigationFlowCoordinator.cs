using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Interop;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using HMUI;
using Zenject;

namespace BeatLeader {
    internal class HeckNavigationFlowCoordinator : FlowCoordinator, IInitializable, IReplayerViewNavigator {
        #region Injection

        [Inject] private readonly DiContainer _container = null!;
        [Inject] private readonly ReplayerMenuLoader _replayerMenuLoader = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;

        #endregion

        #region Impl

        private ViewController _setterViewController = null!;
        private Action? _originalFinishedDelegate;
        private Replay? _pendingReplay;
        private Player? _pendingPlayer;
        private bool _alternativeLoading;

        private static MethodInfo _initMethod = null!;
        private static FieldInfo _finishedEventField = null!;

        public void Initialize() {
            var type = HeckInterop.SetterViewControllerType!;
            _initMethod = type.GetMethod("Init", ReflectionUtils.DefaultFlags)!;
            _finishedEventField = type.GetField("Finished", ReflectionUtils.DefaultFlags)!;
            _setterViewController = (ViewController)_container.Resolve(HeckInterop.PlayViewControllerType);
        }

        public async Task NavigateToReplayAsync(FlowCoordinator flowCoordinator, Replay replay, Player player, bool alternative) {
            _pendingReplay = replay;
            _pendingPlayer = player;
            _alternativeLoading = alternative;
            //loading beatmap data
            var info = replay.info;
            var (level, key) = await _replayerMenuLoader.LoadBeatmapAsync(
                info.hash,
                info.mode,
                info.difficulty,
                CancellationToken.None
            );
            //initializing
            var startData = HeckInterop.CreateStartData(
                info.mode,
                key!.Value,
                level!,
                null,
                GameplayModifiers.noModifiers,
                _playerDataModel.playerData.playerSpecificSettings
            );
            //presenting
            var shouldPresent = (bool)_initMethod.Invoke(_setterViewController, new[] { startData });
            if (shouldPresent) {
                _originalFinishedDelegate = (Action)_finishedEventField.GetValue(_setterViewController);
                _finishedEventField.SetValue(_setterViewController, (Action)HandleSetterViewControllerFinish);
                flowCoordinator.PresentFlowCoordinator(this);
            } else {
                StartReplay(false);
            }
        }

        private async void StartReplay(bool fromFlowCoordinator) {
            var callback = fromFlowCoordinator ? (Action)DismissSetterViewController : null;
            if (_alternativeLoading) {
                await _replayerMenuLoader.StartReplayFromLeaderboardAsync(_pendingReplay!, _pendingPlayer!, finishCallback: callback);
            } else {
                await _replayerMenuLoader.StartReplayAsync(_pendingReplay!, _pendingPlayer!, finishCallback: callback);
            }
        }

        #endregion

        #region FlowCoordinator

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                showBackButton = true;
                SetTitle("Heck");
            }
            ProvideInitialViewControllers(_setterViewController);
        }

        public override void BackButtonWasPressed(ViewController viewController) {
            _parentFlowCoordinator.DismissFlowCoordinator(this);
        }

        #endregion

        #region Callbacks

        private void DismissSetterViewController() {
            _finishedEventField.SetValue(_setterViewController, _originalFinishedDelegate);
            _parentFlowCoordinator.DismissFlowCoordinator(this, immediately: true);
        }

        private void HandleSetterViewControllerFinish() {
            StartReplay(true);
        }

        #endregion
    }
}