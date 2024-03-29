﻿using System;
using BeatLeader.Components;
using BeatLeader.Manager;
using BeatLeader.ViewControllers;
using BeatSaberMarkupLanguage;
using HMUI;
using Zenject;

namespace BeatLeader {
    internal class BeatLeaderFlowCoordinator : FlowCoordinator {
        [Inject] private readonly MainFlowCoordinator _mainFlowCoordinator = null!;
        [Inject] private readonly SoloFreePlayFlowCoordinator _soloFlowCoordinator = null!;
        
        [Inject] private readonly ReplayLaunchViewController _replayLaunchViewController = null!;

        public event Action? BackButtonWasPressedEvent;
        
        private FlowCoordinator? _parentCoordinator;

        #region Init & Dispose

        private void Awake() {
            LeaderboardEvents.MenuButtonWasPressedEvent += PresentFromLeaderboard;
        }

        private void OnDestroy() {
            LeaderboardEvents.MenuButtonWasPressedEvent -= PresentFromLeaderboard;
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (!firstActivation) return;
            showBackButton = true;
            SetTitle(Plugin.PluginId);
            ProvideInitialViewControllers(_replayLaunchViewController);
        }

        public override void BackButtonWasPressed(ViewController topController) {
            BackButtonWasPressedEvent?.Invoke();
            Dismiss();
        }

        #endregion

        #region Present & Dismiss

        private void PresentFromLeaderboard() {
            Present(true);
        }

        public void Present(bool fromLeaderboard) {
            _parentCoordinator = fromLeaderboard ? _soloFlowCoordinator : _mainFlowCoordinator;
            _parentCoordinator.PresentFlowCoordinator(this);
        }

        public void Dismiss() {
            _parentCoordinator?.DismissFlowCoordinator(this);
            _parentCoordinator = null;
        }

        #endregion
    }
}