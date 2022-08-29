using System.Linq;
using BeatLeader.API.Methods;
using BeatLeader.DataManager;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Components {
    internal class Pagination : ReeUIComponentV2 {
        #region Initialize/Dispose

        protected override void OnInitialize() {
            FlipUpButton();
            ScoresRequest.AddStateListener(OnScoreRequestStateChanged);
        }

        protected override void OnDispose() {
            ScoresRequest.RemoveStateListener(OnScoreRequestStateChanged);
        }

        #endregion

        #region Events

        private void OnScoreRequestStateChanged(API.RequestState state, Paged<Score> result, string failReason) {
            if (state is not API.RequestState.Finished) {
                DisableAllInteraction();
                return;
            }
            
            OnScoresFetched(result);
        }

        private void OnScoresFetched(Paged<Score> scoresData) {
            if (scoresData.metadata == null) {
                Plugin.Log.Error("scoresData.metadata is null!");
                DisableAllInteraction();
                return;
            }

            UpInteractable = scoresData.metadata.page > 1;
            AroundInteractable = scoresData.selection != null && !scoresData.data.Any(it => ProfileManager.IsCurrentPlayer(it.player));
            DownInteractable = scoresData.metadata.page * scoresData.metadata.itemsPerPage < scoresData.metadata.total;
        }

        private void DisableAllInteraction() {
            UpInteractable = false;
            AroundInteractable = false;
            DownInteractable = false;
        }

        #endregion

        #region Interactable

        private bool _upInteractable;

        private bool UpInteractable {
            get => _upInteractable;
            set {
                _upInteractable = value;
                SetColors(_upComponent, value);
            }
        }

        private bool _aroundInteractable;

        private bool AroundInteractable {
            get => _aroundInteractable;
            set {
                _aroundInteractable = value;
                SetColors(_aroundComponent, value);
            }
        }

        private bool _downInteractable;

        private bool DownInteractable {
            get => _downInteractable;
            set {
                _downInteractable = value;
                SetColors(_downComponent, value);
            }
        }

        #endregion

        #region Colors

        private static readonly Color HoveredColor = new(0.5f, 0.5f, 0.5f);
        private static readonly Color ActiveColor = new(1.0f, 1.0f, 1.0f);
        private static readonly Color InactiveColor = new(0.2f, 0.2f, 0.2f);

        private static void SetColors(ClickableImage image, bool interactable) {
            if (interactable) {
                image.DefaultColor = ActiveColor;
                image.HighlightColor = HoveredColor;
            } else {
                image.DefaultColor = InactiveColor;
                image.HighlightColor = InactiveColor;
            }
        }

        #endregion

        #region Components

        [UIComponent("up-component"), UsedImplicitly]
        private ClickableImage _upComponent;

        [UIComponent("around-component"), UsedImplicitly]
        private ClickableImage _aroundComponent;

        [UIComponent("down-component"), UsedImplicitly]
        private ClickableImage _downComponent;

        private void FlipUpButton() {
            _upComponent.transform.Rotate(0, 0, 180);
        }

        #endregion

        #region Callbacks

        [UIAction("up-on-click"), UsedImplicitly]
        private void UpOnClick() {
            if (!UpInteractable) return;
            LeaderboardEvents.NotifyUpButtonWasPressed();
        }

        [UIAction("around-on-click"), UsedImplicitly]
        private void AroundOnClick() {
            if (!AroundInteractable) return;
            LeaderboardEvents.NotifyAroundButtonWasPressed();
        }

        [UIAction("down-on-click"), UsedImplicitly]
        private void DownOnClick() {
            if (!DownInteractable) return;
            LeaderboardEvents.NotifyDownButtonWasPressed();
        }

        #endregion
    }
}