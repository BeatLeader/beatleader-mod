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
            
            _settingsPanel = ReeUIComponentV2.Instantiate<ChristmasTreeSettingsPanel>(null!);
            _settingsPanel.ManualInit(null!);

            ChristmasTreeRequest.AddStateListener(HandleTreeRequestState);
            ChristmasTreeRequest.SendRequest();

            LeaderboardEvents.TreeButtonWasPressedEvent += HandleTreeButtonClicked;
            SoloFlowCoordinatorPatch.PresentedEvent += HandleCoordinatorPresented;
        }

        public void LateDispose() {
            ChristmasTreeRequest.RemoveStateListener(HandleTreeRequestState);
            LeaderboardEvents.TreeButtonWasPressedEvent -= HandleTreeButtonClicked;
            SoloFlowCoordinatorPatch.PresentedEvent -= HandleCoordinatorPresented;
        }

        #endregion

        #region Tree

        private ChristmasTree _christmasTree = null!;
        private ChristmasTreeSettingsPanel _settingsPanel = null!;

        private void SpawnTree() {
            var prefab = BundleLoader.ChristmasTree;
            var instance = Object.Instantiate(prefab, null, false);
            instance.MoveTo(new Vector3(2.7f, 0f, 4f), true);
            _christmasTree = instance;
            _settingsPanel.Setup(_christmasTree);
        }

        private void HandleTreeRequestState(API.RequestState state, ChristmasTreeSettings settings, string? failReason) {
            if (state != API.RequestState.Finished) {
                return;
            }
            SpawnTree();
            _christmasTree.LoadSettings(settings);
        }

        #endregion

        #region Callbacks

        private void HandleCoordinatorPresented() {
            _christmasTree.Present();
        }

        private void HandleTreeButtonClicked() {
            _settingsPanel.Present();
        }

        #endregion
    }
}