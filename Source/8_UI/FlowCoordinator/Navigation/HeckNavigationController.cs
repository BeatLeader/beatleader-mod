using System;
using System.Collections.Generic;
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
    internal class HeckNavigationController : IInitializable, IDisposable, IReplayerViewNavigator {
        #region Injection

        [Inject] private readonly DiContainer _container = null!;
        [Inject] private readonly ReplayerMenuLoader _replayerMenuLoader = null!;
        [Inject] private readonly ReplayerViewNavigator _replayerViewNavigator = null!;
        [Inject] private readonly PlayerDataModel _playerDataModel = null!;

        #endregion

        #region Impl

        private Replay? _pendingReplay;
        private Player? _pendingPlayer;
        private bool _alternativeLoading;

        public async Task NavigateToReplayAsync(FlowCoordinator flowCoordinator, Replay replay, Player player, bool tryLoadSelectedMap) {
            _pendingReplay = replay;
            _pendingPlayer = player;
            _alternativeLoading = tryLoadSelectedMap;

            // Loading beatmap data
            var info = replay.info;
            var level = await _replayerMenuLoader.LoadBeatmapAsync(
                info.hash,
                info.mode,
                info.difficulty,
                CancellationToken.None
            );

            // Initializing
            var startData = HeckInterop.CreateStartData(
                info.mode,
                level.Key,
                level.Level,
                null,
                GameplayModifiers.noModifiers,
                _playerDataModel.playerData.playerSpecificSettings
            );

            HeckInitViewManager(flowCoordinator, startData);
        }

        private async Task StartReplay() {
            if (_alternativeLoading) {
                await _replayerMenuLoader.StartReplayFromLeaderboardAsync(_pendingReplay!, _pendingPlayer!);
            } else {
                await _replayerMenuLoader.StartReplayAsync(_pendingReplay!, _pendingPlayer!);
            }
        }

        #endregion

        #region Heck Reflection

        private static MethodInfo? _playViewDataOnPlayMethod;
        private static MethodInfo? _playViewManagerStartMethod;
        private static MethodInfo? _playViewManagerEarlyDismissMethod;
        private static MethodInfo? _playViewManagerActivateMethod;
        private static MethodInfo? _playViewManagerInitMethod;
        private static FieldInfo? _playViewManagerFlowCoordinatorField;
        private static FieldInfo? _playViewManagerViewControllersField;

        private HarmonyAutoPatch _playViewManagerStartPatch = null!;
        private HarmonyAutoPatch _playViewManagerDismissPatch = null!;
        private HarmonyAutoPatch _playViewManagerActivatePatch = null!;

        private static HeckNavigationController? _heckNavigationController;
        private static object? _originalFlowCoordinator;
        private static object? _customFlowCoordinator;
        private object _playViewManager = null!;
        private static bool _presentingHeck;

        public void Initialize() {
            var type = HeckInterop.PlayViewManagerType!;

            _playViewManagerStartMethod ??= type.GetMethodThrowable("StartStandard");
            _playViewManagerEarlyDismissMethod ??= type.GetMethodThrowable("EarlyDismiss");
            _playViewManagerInitMethod ??= type.GetMethodThrowable("Init");
            _playViewManagerActivateMethod ??= type.GetMethodThrowable("Activate");
            
            _playViewManagerFlowCoordinatorField ??= type.GetFieldThrowable("_flowCoordinator");
            _playViewManagerViewControllersField ??= type.GetFieldThrowable("_viewControllers");

            _playViewDataOnPlayMethod ??= HeckInterop.PlayViewControllerDataType!.GetMethodThrowable("InvokeOnPlay");

            _playViewManagerStartPatch = new HarmonyPatchDescriptor(
                _playViewManagerStartMethod,
                prefix: typeof(HeckNavigationController).GetMethodThrowable(nameof(HeckStartStandardPrefix))
            );

            _playViewManagerDismissPatch = new HarmonyPatchDescriptor(
                _playViewManagerEarlyDismissMethod,
                postfix: typeof(HeckNavigationController).GetMethodThrowable(nameof(HeckEarlyDismissPostfix))
            );
            
            _playViewManagerActivatePatch = new HarmonyPatchDescriptor(
                _playViewManagerActivateMethod,
                prefix: typeof(HeckNavigationController).GetMethodThrowable(nameof(HeckActivatePrefix))
            );

            _playViewManager = _container.Resolve(HeckInterop.PlayViewManagerType);
            _heckNavigationController = this;
        }

        public void Dispose() {
            _playViewManagerStartPatch.Dispose();
            _playViewManagerDismissPatch.Dispose();
            _heckNavigationController = null;
        }

        private void HeckInitViewManager(FlowCoordinator flowCoordinator, object data) {
            _presentingHeck = true;
            _customFlowCoordinator = flowCoordinator;

            _playViewManagerInitMethod!.Invoke(_playViewManager, [data, false]);
        }

        private static void HeckActivatePrefix(object __instance) {
            if (!_presentingHeck) {
                return;
            }
            
            _originalFlowCoordinator = _playViewManagerFlowCoordinatorField!.GetValue(__instance);
            _playViewManagerFlowCoordinatorField.SetValue(__instance, _customFlowCoordinator);
        }

        private static bool HeckStartStandardPrefix(object __instance) {
            if (!_presentingHeck) {
                return true;
            }
            
            _presentingHeck = false;
            _playViewManagerFlowCoordinatorField!.SetValue(__instance, _originalFlowCoordinator);
            
            var viewControllers = (object[])_playViewManagerViewControllersField!.GetValue(__instance);

            foreach (var viewController in viewControllers) {
                _playViewDataOnPlayMethod!.Invoke(viewController, []);
            }

            _ = _heckNavigationController!.StartReplay().RunCatching();

            return false;
        }

        private static void HeckEarlyDismissPostfix(object __instance) {
            _presentingHeck = false;
            _playViewManagerFlowCoordinatorField!.SetValue(__instance, _originalFlowCoordinator);
        }

        #endregion

        #region Adapter

        public void NavigateToReplayManager(FlowCoordinator flowCoordinator, IReplayHeader header) {
            _replayerViewNavigator.NavigateToReplayManager(flowCoordinator, header);
        }

        public void NavigateToBattleRoyale(
            FlowCoordinator flowCoordinator,
            BeatmapLevelWithKey level,
            IReadOnlyCollection<IReplayHeader> plays,
            bool allowChanges,
            bool clearOnExit
        ) {
            _replayerViewNavigator.NavigateToBattleRoyale(flowCoordinator, level, plays, allowChanges, clearOnExit);
        }

        #endregion
    }
}