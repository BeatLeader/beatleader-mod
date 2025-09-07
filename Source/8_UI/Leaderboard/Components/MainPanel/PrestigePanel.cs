using BeatLeader.API;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class PrestigePanel : AbstractReeModal<object> {
        #region Init / Dispose

        private FireworksController fireworksController = null;

        protected override void OnInitialize() {
            base.OnInitialize();
            InitializePrestigeButtons();
            UserRequest.StateChangedEvent += OnProfileRequestStateChanged;
            UploadReplayRequest.StateChangedEvent += OnUploadStateChanged;

            fireworksController = UnityEngine.Object.FindObjectsByType<FireworksController>(FindObjectsSortMode.None).FirstOrDefault();
        }

        protected override void OnDispose() {
            UserRequest.StateChangedEvent -= OnProfileRequestStateChanged;
            UploadReplayRequest.StateChangedEvent -= OnUploadStateChanged;
        }

        #endregion

        #region Events

        private void OnProfileRequestStateChanged(WebRequests.IWebRequest<Player> instance, WebRequests.RequestState state, string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Finished:
                    Player player = instance.Result;
                    if (player.level == 100) {
                        _PrestigeYesButton.interactable = true;
                    } else {
                        _PrestigeYesButton.interactable = false;
                    }
                    break;
                default: return;
            }
        }

        private void OnUploadStateChanged(WebRequests.IWebRequest<ScoreUploadResponse> instance, WebRequests.RequestState state,
            string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Finished:
                    if (instance.Result.Status != ScoreUploadStatus.Error) {
                        Player player = instance.Result.Score.Player;
                        if (player.level == 100) {
                            _PrestigeYesButton.interactable = true;
                        } else {
                            _PrestigeYesButton.interactable = false;
                        }
                    }
                    break;
                default: return;
            }
        }

        #endregion

        #region Prestige

        private void RequestPrestige() {
            PrestigeRequest.Send();
            _PrestigeYesButton.interactable = false;
            if (fireworksController != null) {
                Task.Run(() => Fireworks(5));
            }
            Close();
        }

        private async Task<bool> Fireworks(double duration) {
            fireworksController.enabled = true;
            await Task.Delay(TimeSpan.FromSeconds(duration));
            fireworksController.enabled = false;
            return true;
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