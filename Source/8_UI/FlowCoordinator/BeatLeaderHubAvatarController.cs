using System;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;
using BeatSaber.AvatarCore;
using BeatSaber.BeatAvatarAdapter.AvatarEditor;
using BeatSaber.BeatAvatarSDK;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BeatLeaderHubAvatarController : MonoBehaviour {
        #region Injection

        [Inject] private readonly HierarchyManager _hierarchyManager = null!;
        [Inject] private readonly BeatLeaderHubFlowCoordinator _hubFlowCoordinator = null!;
        [Inject] private readonly BeatAvatarLoader _beatAvatarLoader = null!;
        [Inject] private readonly BeatLeaderHubTheme _hubTheme = null!;
        [Inject] private readonly DiContainer _container = null!;

        #endregion

        #region Setup

        private BeatAvatarEditorFlowCoordinator _editAvatarFlowCoordinator = null!;
        private BeatAvatarEditorViewController _editAvatarViewController = null!;

        private EditAvatarFloatingButton _editAvatarButton = null!;
        private MenuBeatAvatarController _avatarController = null!;
        private Transform _screenTransform = null!;

        private AvatarData? _avatarData;

        private async void Awake() {
            _editAvatarButton = new EditAvatarFloatingButton {
                ContentTransform = {
                    localPosition = new(0f, 1f, 0.15f),
                    localEulerAngles = new(0f, 180f, 0f),
                    localScale = 0.02f * Vector3.one
                },
                Interactable = false,
                Enabled = false,
                OnClick = PresentEditFlowCoordinator
            };
            _editAvatarButton.Setup(_hubTheme.MenuButtonsTheme);
            _editAvatarButton.Use(transform);
            _hubFlowCoordinator.FlowCoordinatorPresentedEvent += Present;
            _hubFlowCoordinator.FlowCoordinatorDismissedEvent += Dismiss;
            //
            _editAvatarFlowCoordinator = await _beatAvatarLoader.CreateEditorFlowCoordinator();
            _avatarController = _beatAvatarLoader.CreateMenuAvatar(transform);
            SetupEditAvatarFlowCoordinator();
            //
            var screenSystem = _hierarchyManager.GetComponent<ScreenSystem>();
            _screenTransform = screenSystem.mainScreen.transform;
            LoadAvatar();
        }

        #endregion

        #region Avatar

        private async void UploadAvatar() {
            _avatarController.SetVisuals(_avatarData);
            _avatarController.SetLoading(true);
            var player = ProfileManager.Profile!;
            var settings = AvatarSettings.FromAvatarData(_avatarData);
            //request
            var request = UpdateAvatarRequest.Send(player.id, settings);
            await request.Join();
            if (request.RequestState is WebRequests.RequestState.Failed) {
                await LoadAvatarFromPlayerAsync(player, true);
            }
            //forcing player to update avatar
            await player.GetBeatAvatarAsync(true);
            _avatarController.SetLoading(false);
        }

        private async void LoadAvatar() {
            _avatarController.SetLoading(true);
            await ProfileManager.WaitUntilProfileLoad();
            var player = ProfileManager.Profile;
            if (player != null) {
                await LoadAvatarFromPlayerAsync(player, false);
                _editAvatarButton.Interactable = true;
            }
            _avatarController.SetLoading(false);
        }

        private async Task LoadAvatarFromPlayerAsync(IPlayer player, bool bypassCache) {
            var settings = await player.GetBeatAvatarAsync(bypassCache);
            _avatarData = settings.ToAvatarData();
            _avatarController.SetVisuals(_avatarData, true);
        }

        #endregion

        #region Present & Dismiss

        private void Present() {
            _avatarController.Present();
            var screenPos = _screenTransform.position;
            transform.position = new Vector3(screenPos.x - 2f, 0f, screenPos.z - 1f);
            transform.eulerAngles = new(0f, 135f, 0f);
            _editAvatarButton.Present();
        }

        private void Dismiss() {
            _avatarController.Hide();
            _editAvatarButton.Hide();
        }

        #endregion

        #region EditAvatarFlowCoordinator

        private readonly AvatarDataModel _customAvatarDataModel = new();

        private void PresentEditFlowCoordinator() {
            _customAvatarDataModel._avatarData = _avatarData;
            _editAvatarFlowCoordinator.Setup(AvatarEditorFlowCoordinator.EditMode.Edit);
            _hubFlowCoordinator.PresentFlowCoordinator(_editAvatarFlowCoordinator);
        }

        private void SetupEditAvatarFlowCoordinator() {
            _container.Inject(_customAvatarDataModel);
            _editAvatarViewController = _editAvatarFlowCoordinator._beatAvatarEditorViewController;
            _editAvatarViewController.SetField("_avatarDataModel", _customAvatarDataModel);
            _editAvatarFlowCoordinator.SetField("_avatarDataModel", _customAvatarDataModel);
            _editAvatarFlowCoordinator.didFinishEvent += HandleFlowCoordinatorEditFinished;
        }

        private void HandleFlowCoordinatorEditFinished(AvatarEditorFlowCoordinator _, IAvatarSystemMetadata _1, AvatarEditorFlowCoordinator.FinishAction finishAction) {
            if (finishAction is AvatarEditorFlowCoordinator.FinishAction.Apply) {
                UploadAvatar();
            }
            _editAvatarFlowCoordinator.DismissSelf();
        }

        #endregion
    }
}