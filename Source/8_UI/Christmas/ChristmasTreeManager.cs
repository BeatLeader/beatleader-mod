using System.Threading;
using BeatLeader.API.Methods;
using BeatLeader.Components;
using BeatLeader.Manager;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader {
    internal class ChristmasTreeManager : IInitializable, ILateDisposable {
        #region Init

        public void Initialize() {
            SpawnTree();

            _settingsPanel = ReeUIComponentV2.Instantiate<ChristmasTreeSettingsPanel>(null!);
            _settingsPanel.ManualInit(null!);
            _settingsPanel.Setup(_christmasTree);

            ChristmasTreeRequest.AddStateListener(HandleTreeRequestState);
            ChristmasTreeRequest.SendRequest();
            LeaderboardEvents.TreeButtonWasPressedEvent += HandleTreeButtonClicked;
            SoloFlowCoordinatorPatch.PresentedEvent += HandleCoordinatorPresented;
            SoloFlowCoordinatorPatch.DismissedEvent += HandleCoordinatorDismissed;
        }

        public void LateDispose() {
            ChristmasTreeRequest.RemoveStateListener(HandleTreeRequestState);
            LeaderboardEvents.TreeButtonWasPressedEvent -= HandleTreeButtonClicked;
            SoloFlowCoordinatorPatch.PresentedEvent -= HandleCoordinatorPresented;
            SoloFlowCoordinatorPatch.DismissedEvent -= HandleCoordinatorDismissed;
        }

        #endregion

        #region Tree

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private ChristmasTree _christmasTree = null!;
        private ChristmasTreeSettingsPanel _settingsPanel = null!;

        private void SpawnTree() {
            var prefab = BundleLoader.ChristmasTree;
            var instance = Object.Instantiate(prefab, null, false);
            instance.transform.position = new Vector3(2.7f, 0f, 4f);
            instance.transform.localScale = Vector3.one * 1.7f;
            _christmasTree = instance;
        }

        private async void LoadTreeSettings(ChristmasTreeSettings settings) {
            await _semaphore.WaitAsync();
            await _christmasTree.LoadOrnaments(settings);
            _semaphore.Release();
        }

        private void HandleTreeRequestState(API.RequestState state, ChristmasTreeSettings settings, string? failReason) {
            if (state != API.RequestState.Finished) {
                return;
            }
            LoadTreeSettings(settings);
            _settingsPanel.LoadSettings(settings);
        }

        #endregion

        #region Callbacks

        private void HandleCoordinatorPresented() {
            _christmasTree.Present();
        }

        private void HandleCoordinatorDismissed() {
            _christmasTree.Dismiss();
        }

        private void HandleTreeButtonClicked() {
            _settingsPanel.Present();
        }

        #endregion
    }
}