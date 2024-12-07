using System.Threading;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader {
    internal class ChristmasTreeManager : IInitializable, ILateDisposable {
        #region Init

        public void Initialize() {
            SpawnTree();
            ChristmasTreeRequest.AddStateListener(HandleTreeRequestState);
            ChristmasTreeRequest.SendRequest();
            SoloFlowCoordinatorPatch.PresentedEvent += HandleCoordinatorPresented;
            SoloFlowCoordinatorPatch.DismissedEvent += HandleCoordinatorDismissed;
        }

        public void LateDispose() {
            ChristmasTreeRequest.RemoveStateListener(HandleTreeRequestState);
            SoloFlowCoordinatorPatch.PresentedEvent -= HandleCoordinatorPresented;
            SoloFlowCoordinatorPatch.DismissedEvent -= HandleCoordinatorDismissed;
        }

        #endregion

        #region Tree

        private readonly SemaphoreSlim _semaphore = new(1,1);
        private ChristmasTree _christmasTree = null!;

        private void SpawnTree() {
            var prefab = BundleLoader.ChristmasTree;
            var instance = Object.Instantiate(prefab, null, false);
            instance.transform.position = new Vector3(2.7f, 0f, 4f);
            _christmasTree = instance;
        }

        private async void LoadTreeSettings(ChristmasTreeSettings settings) {
            await _semaphore.WaitAsync();
            await _christmasTree.LoadOrnaments(settings);
            _semaphore.Release();
        }

        private void HandleTreeRequestState(API.RequestState state, ChristmasTreeSettings settings, string? failReason) {
            if (state == API.RequestState.Finished) {
                LoadTreeSettings(settings);
            }
        }
        
        #endregion

        #region Callbacks

        private void HandleCoordinatorPresented() {
            _christmasTree.Present();
        }

        private void HandleCoordinatorDismissed() {
            _christmasTree.Dismiss();
        }

        #endregion
    }
}