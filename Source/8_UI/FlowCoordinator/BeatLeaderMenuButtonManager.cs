﻿using System;
using BeatSaberMarkupLanguage.MenuButtons;
using UnityEngine;
using Zenject;

namespace BeatLeader {
    internal class BeatLeaderMenuButtonManager : MonoBehaviour {
        [Inject] private readonly BeatLeaderFlowCoordinator _flowCoordinator = null!;
        
        #region MenuButton

        private const string Description = "Opens BeatLeader replays panel";

        public static bool MenuButtonEnabled {
            get => ConfigFileData.Instance.MenuButtonEnabled;
            set => ConfigFileData.Instance.MenuButtonEnabled = value;
        }
        
        private static event Action? MenuButtonClickedEvent;
        
        private static readonly MenuButton menuButton = new(
            Plugin.PluginId, Description, 
            () => MenuButtonClickedEvent?.Invoke());

        #endregion

        #region Init & Dispose

        private void Start() {
            if (!MenuButtonEnabled) return;
            MenuButtons.Instance.RegisterButton(menuButton);
            MenuButtonClickedEvent += HandleMenuButtonClicked;
        }

        private void OnDestroy() {
            MenuButtonClickedEvent -= HandleMenuButtonClicked;  
        }

        #endregion

        #region Callbacks

        private void HandleMenuButtonClicked() {
            _flowCoordinator.Present(false);
        }

        #endregion
    }
}