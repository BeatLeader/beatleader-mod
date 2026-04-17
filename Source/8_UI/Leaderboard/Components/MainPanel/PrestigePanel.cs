using BeatLeader.API;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader.Components {
    internal class PrestigePanel : AbstractReeModal<object> {
        #region Init / Dispose

        private FireworksController fireworksController = null;

        [UIComponent("primaryText"), UsedImplicitly]
        private TextMeshProUGUI primaryText;

        [UIComponent("secondaryText"), UsedImplicitly]
        private TextMeshProUGUI secondaryText;

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
                    UpdatePanelContent(instance.Result);
                    break;
                default: return;
            }
        }

        private void OnUploadStateChanged(WebRequests.IWebRequest<ScoreUploadResponse> instance, WebRequests.RequestState state,
            string? failReason) {
            switch (state) {
                case WebRequests.RequestState.Finished:
                    if (instance.Result.Status != ScoreUploadStatus.Error) {
                        UpdatePanelContent(instance.Result.Score.Player);
                    }
                    break;
                default: return;
            }
        }

        private void UpdatePanelContent(Player player) {
            bool canPrestige = player.level == 100;

            primaryText.SetText(
                    canPrestige
                        ? "Congratulations!\nYou've reached level 100 and are now ready to Prestige."
                        : "Gain experience points by playing any maps, even for failing! Reach level 100 to be able to prestige into the next iteration.");

            secondaryText.SetText(
                    canPrestige
                        ? $"This will reset your level and you will reach Prestige {player.prestige + 1}. Are you ready?"
                        : "To get more points, pass maps always with 95+% accuracy. But even playing with 90% accuracy will give you almost the full xp for the time played.\nJust play more!");

            _PrestigeYesButton.gameObject.active = canPrestige;
            _PrestigeYesButton.interactable = canPrestige;

            _PrestigeNoButton.GetComponentInChildren<TextMeshProUGUI>().SetText(canPrestige ? "No" : "Close");
        }

        public static event Action PrestigeWasPressedEvent;

        #endregion

        #region Prestige

        private void RequestPrestige() {
            PrestigeRequest.Send();
            PrestigeWasPressedEvent?.Invoke();
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