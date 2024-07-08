using System;
using BeatSaberMarkupLanguage.MenuButtons;
using UnityEngine;
using Zenject;

namespace BeatLeader {
    internal class BeatLeaderMenuButtonManager : MonoBehaviour {
        [Inject] private readonly BeatLeaderHubFlowCoordinator _hubFlowCoordinator = null!;
        
        #region MenuButton

        private const string Description = "Opens BeatLeader Hub";

        public static bool MenuButtonEnabled {
            get => ConfigFileData.Instance.MenuButtonEnabled;
            set => ConfigFileData.Instance.MenuButtonEnabled = value;
        }
        
        private static event Action? MenuButtonClickedEvent;
        
        private static readonly MenuButton menuButton = new(
            "BEATLEADER HUB", Description, 
            () => MenuButtonClickedEvent?.Invoke());

        #endregion

        #region Init & Dispose

        private void Start() {
            if (!MenuButtonEnabled) return;
            MenuButtons.Instance.RegisterButton(menuButton);
            MenuButtonClickedEvent += HandleMenuButtonClicked;
        }

        private void OnDestroy() {
            MenuButtons.Instance.UnregisterButton(menuButton);
            MenuButtonClickedEvent -= HandleMenuButtonClicked;  
        }

        #endregion

        #region Callbacks

        private void HandleMenuButtonClicked() {
            _hubFlowCoordinator.Present(false);
        }

        #endregion
    }
}