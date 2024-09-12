using System;
using System.Linq;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.DataManager;
using BeatLeader.Models;
using BeatLeader.Replayer.Emulation;
using BeatLeader.UI.Reactive.Components;
using BeatSaberMarkupLanguage;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.UI.Hub {
    internal class BeatLeaderHubAvatarController : MonoBehaviour {
        #region Injection

        [Inject] private readonly HierarchyManager _hierarchyManager = null!;
        [Inject] private readonly BeatLeaderHubFlowCoordinator _hubFlowCoordinator = null!;
        [Inject] private readonly AvatarLoader _avatarLoader = null!;
        [Inject] private readonly BeatLeaderHubTheme _hubTheme = null!;
        [Inject] private readonly EditAvatarFlowCoordinator _editAvatarFlowCoordinator = null!;
        [Inject] private readonly EditAvatarViewController _editAvatarViewController = null!;
        [Inject] private readonly DiContainer _container = null!;

        #endregion

        #region Setup

        private class CustomAvatarDataModel : AvatarDataModel {
            public override void Save() { }
            public override void Load() { }
        }

        private EditAvatarFloatingButton _editAvatarButton = null!;
        private AvatarController _avatarController = null!;
        private Transform _screenTransform = null!;

        private readonly PlayerDataModel _playerDataModel = new();
        private readonly AvatarDataModel _avatarDataModel = new CustomAvatarDataModel();
        private AvatarData? _avatarData;

        private void Awake() {
            _editAvatarButton = new EditAvatarFloatingButton {
                ContentTransform = {
                    localPosition = new(0f, 1f, 0.15f),
                    localEulerAngles = new(0f, 180f, 0f)
                },
                Interactable = false,
                Enabled = false,
                OnClick = PresentEditFlowCoordinator
            };
            _editAvatarButton.Setup(_hubTheme.MenuButtonsTheme);
            _editAvatarButton.Use(transform);
            //
            var playerDataManager = Resources.FindObjectsOfTypeAll<PlayerDataFileManagerSO>().First();
            _playerDataModel.SetField("_playerDataFileManager", playerDataManager);
            _playerDataModel.ResetData();
            _playerDataModel.playerData.MarkAvatarCreated();
            _container.Inject(_avatarDataModel);
            //
            var screenSystem = _hierarchyManager.GetComponent<ScreenSystem>();
            _screenTransform = screenSystem.mainScreen.transform;
            _avatarController = _avatarLoader.CreateAvatar(transform);
            _hubFlowCoordinator.FlowCoordinatorPresentedEvent += Present;
            _hubFlowCoordinator.FlowCoordinatorDismissedEvent += Dismiss;
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
            await player.GetAvatarAsync(true);
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
            var settings = await player.GetAvatarAsync(bypassCache);
            _avatarData = settings.ToAvatarData();
            _avatarDataModel.avatarData = _avatarData;
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

        private PlayerDataModel? _originalPlayerDataModel;
        private AvatarDataModel? _originalAvatarDataModel;
        private Action<EditAvatarFlowCoordinator, EditAvatarFlowCoordinator.EditAvatarType>? _originalDidFinishDelegate;

        private void PresentEditFlowCoordinator() {
            //caching original values
            _originalPlayerDataModel = _editAvatarViewController.GetField<PlayerDataModel, EditAvatarViewController>("_playerDataModel");
            _originalAvatarDataModel = _editAvatarViewController.GetField<AvatarDataModel, EditAvatarViewController>("_avatarDataModel");
            _originalDidFinishDelegate = _editAvatarFlowCoordinator.GetField<Action<EditAvatarFlowCoordinator, EditAvatarFlowCoordinator.EditAvatarType>, EditAvatarFlowCoordinator>("didFinishEvent");
            //applying custom values
            _editAvatarViewController.SetField("_playerDataModel", _playerDataModel);
            _editAvatarViewController.SetField("_avatarDataModel", _avatarDataModel);
            _editAvatarFlowCoordinator.SetField("didFinishEvent", (Action<EditAvatarFlowCoordinator, EditAvatarFlowCoordinator.EditAvatarType>)HandleFlowCoordinatorEditFinished);
            _editAvatarFlowCoordinator.SetField("_avatarDataModel", _avatarDataModel);
            _editAvatarViewController.didFinishEvent += HandleEditFinished;
            _editAvatarFlowCoordinator.Setup(EditAvatarFlowCoordinator.EditAvatarType.Edit);
            _hubFlowCoordinator.PresentFlowCoordinator(_editAvatarFlowCoordinator);
        }

        private void HandleEditFinished(EditAvatarViewController.FinishAction action) {
            _editAvatarViewController.didFinishEvent -= HandleEditFinished;
            if (action is EditAvatarViewController.FinishAction.Apply) {
                UploadAvatar();
            }
            //restoring original values
            _editAvatarViewController.SetField("_playerDataModel", _originalPlayerDataModel);
            _editAvatarViewController.SetField("_avatarDataModel", _originalAvatarDataModel);
        }

        private void HandleFlowCoordinatorEditFinished(EditAvatarFlowCoordinator _, EditAvatarFlowCoordinator.EditAvatarType _1) {
            _editAvatarFlowCoordinator.SetField("didFinishEvent", _originalDidFinishDelegate);
            _editAvatarFlowCoordinator.SetField("_avatarDataModel", _originalAvatarDataModel);
            _editAvatarFlowCoordinator.DismissSelf();
            
        }
        
        #endregion
    }
}