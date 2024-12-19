using BeatLeader.API.Methods;
using BeatLeader.Components;
using BeatLeader.Manager;
using BeatLeader.Models;
using BeatLeader.Utils;
using Zenject;
using Object = UnityEngine.Object;

namespace BeatLeader {
    internal class ChristmasTreeManager : IInitializable, ILateDisposable {
        #region Init

        public void Initialize() {
            SpawnTree();
            SpawnSnow();

            _settingsPanel = ReeUIComponentV2.Instantiate<ChristmasTreeSettingsPanel>(null!);
            _settingsPanel.ManualInit(null!);
            _settingsPanel.Setup(_christmasTree);

            ChristmasTreeRequest.AddStateListener(HandleTreeRequestState);
            ChristmasTreeRequest.SendRequest();

            LeaderboardEvents.TreeButtonWasPressedEvent += HandleTreeButtonClicked;
            LeaderboardEvents.TreeEditorWasRequested += HandleTreeEditorWasRequested;
            SoloFlowCoordinatorPatch.PresentedEvent += HandleCoordinatorPresented;
            PluginConfig.ChristmasSettingsUpdatedEvent += HandleChristmasSettingsUpdated;

            ScoreInfoPanel.ScoreInfoPanelEvent += HandleScoreInfoPanelVisibility;

            OthersChristmasTreeRequest.AddStateListener(HandleOthersTreeRequestState);
        }

        

        public void LateDispose() {
            ChristmasTreeRequest.RemoveStateListener(HandleTreeRequestState);
            LeaderboardEvents.TreeButtonWasPressedEvent -= HandleTreeButtonClicked;
            LeaderboardEvents.TreeEditorWasRequested -= HandleTreeEditorWasRequested;
            SoloFlowCoordinatorPatch.PresentedEvent -= HandleCoordinatorPresented;
            PluginConfig.ChristmasSettingsUpdatedEvent -= HandleChristmasSettingsUpdated;

            ScoreInfoPanel.ScoreInfoPanelEvent -= HandleScoreInfoPanelVisibility;
        }

        #endregion

        #region Tree

        private bool CanPresentTree => !_settingsPanel.IsEditorOpened &&
            PluginConfig.ChristmasSettings.TreeEnabled && _treeSettingsLoaded;

        private ChristmasTree _christmasTree = null!;
        private ChristmasTreeSettingsPanel _settingsPanel = null!;
        private bool _treeSettingsLoaded;
        private bool _coordinatorWasPresented;

        private ChristmasTree? _othersTree = null;

        private void SpawnTree() {
            var prefab = BundleLoader.ChristmasTree;
            _christmasTree = Object.Instantiate(prefab, null, false);
        }

        private async void HandleTreeRequestState(API.RequestState state, ChristmasTreeSettings settings, string? failReason) {
            if (state != API.RequestState.Finished) {
                return;
            }
            await _christmasTree.LoadSettings(settings);
            _treeSettingsLoaded = true;
            if (_coordinatorWasPresented && CanPresentTree) {
                _christmasTree.Present();
            }
        }

        private async void HandleOthersTreeRequestState(API.RequestState state, ChristmasTreeSettings settings, string? failReason) {
            if (state != API.RequestState.Finished) {
                return;
            }
            if (_othersTree != null) {
                await _othersTree.LoadSettings(settings, false);
                _othersTree.ScaleTo(1.4f);
                _othersTree.StartSpinning();
            }
        }

        private void HandleScoreInfoPanelVisibility((bool, Score) panelState) {
            Plugin.Log.Critical($"HandleScoreInfoPanelVisibility {panelState.Item1}");
            if (!PluginConfig.ChristmasSettings.OthersTreeEnabled) return;

            if (panelState.Item1) {
                if (_othersTree == null) {
                    var prefab = BundleLoader.ChristmasTree;
                    _othersTree = Object.Instantiate(prefab, null, false);

                    _othersTree.transform.SetLocalPose(new FullSerializablePose {
                        position = new UnityEngine.Vector3(4.2f, 0f, 0.5f),
                        rotation = new UnityEngine.Quaternion(0, 0, 0, 1),
                    });
                    _othersTree.Dismiss();
                }
                OthersChristmasTreeRequest.SendRequest(panelState.Item2.Player.id);
                
            } else {
                if (_othersTree != null) {
                    _othersTree.Dismiss();
                }
            }
        }

        #endregion

        #region Snow

        private SnowController _snow = null!;

        private void SpawnSnow() {
            var prefab = BundleLoader.SnowController;
            _snow = Object.Instantiate(prefab, null, false);

            if (PluginConfig.ChristmasSettings.SnowEnabled) {
                _snow.Play(true);
            } else {
                _snow.Stop();
            }
        }

        #endregion

        #region Callbacks

        private void HandleChristmasSettingsUpdated(ChristmasSettings settings) {
            if (settings.SnowEnabled) {
                _snow.Play(false);
            } else {
                _snow.Stop();
            }
            if (settings.TreeEnabled) {
                _christmasTree.Present();
            } else {
                _christmasTree.Dismiss();
            }
        }

        private void HandleCoordinatorPresented() {
            _coordinatorWasPresented = true;
            if (CanPresentTree) {
                _christmasTree.Present();
            }
        }

        private void HandleTreeButtonClicked() {
            _settingsPanel.Present();
        }

        private void HandleTreeEditorWasRequested() {
            _settingsPanel.Present();
            _settingsPanel.HandleEditorButtonClicked();
        }

        #endregion
    }
}