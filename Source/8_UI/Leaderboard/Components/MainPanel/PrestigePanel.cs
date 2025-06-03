using BeatLeader.API;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class PrestigePanel : AbstractReeModal<object> {
        #region Components
        
        private User user;
        
        #endregion
        
        #region Init / Dispose

        protected override void OnInitialize() {
            base.OnInitialize();
            InitializePrestigeButtons();
            UserRequest.StateChangedEvent += OnProfileRequestStateChanged;
        }

        protected override void OnDispose() {
            UserRequest.StateChangedEvent -= OnProfileRequestStateChanged;
        }

        #endregion

        #region Events
        
        private void OnProfileRequestStateChanged(WebRequests.IWebRequest<User> instance, WebRequests.RequestState state, string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Finished:
                    user = instance.Result;
                    if (user.player.level == 100) {
                        _PrestigeYesButton.interactable = true;
                    } else {
                        _PrestigeYesButton.interactable = false;
                    }
                    break;
                default: return;
            }
        }
        
        #endregion
        
        #region Prestige
        
        private void RequestPrestige() {
            PrestigeRequest.Send();
            Close();
        }

        #endregion

        #region PlaylistButtons

        [UIComponent("prestige-yes-button"), UsedImplicitly]
        private Button _PrestigeYesButton;

        [UIComponent("prestige-no-button"), UsedImplicitly]
        private Button _PrestigeNoButton;

        private void InitializePrestigeButtons() {
            _PrestigeYesButton.onClick.AddListener(() => RequestPrestige());
            _PrestigeYesButton.interactable = false;
            _PrestigeNoButton.onClick.AddListener(() => Close());
        }

        #endregion
    }
}