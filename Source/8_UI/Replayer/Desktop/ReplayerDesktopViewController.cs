using BeatLeader.Models;
using HMUI;
using Reactive;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace BeatLeader.UI.Replayer.Desktop {
    internal class ReplayerDesktopViewController : ViewController {
        #region Injection

        [Inject] private readonly IReplayPauseController _pauseController = null!;
        [Inject] private readonly IReplayFinishController _finishController = null!;
        [Inject] private readonly IReplayTimeController _timeController = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly IVirtualPlayerBodySpawner _bodySpawner = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly ICameraController _cameraController = null!;
        [Inject] private readonly IReplayWatermark _watermark = null!;

        #endregion

        #region Setup

        private ReplayerUIPanel _replayerUIPanel = null!;

        private void Awake() {
            gameObject.AddComponent<GraphicRaycaster>();
            Destroy(gameObject.GetComponent<BaseRaycaster>());
        }

        public override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) {
                _replayerUIPanel = new ReplayerUIPanel();
                _replayerUIPanel.WithRectExpand().Use(transform);
                _replayerUIPanel.Setup(
                    _pauseController,
                    _finishController,
                    _timeController,
                    _playersManager,
                    _cameraController,
                    _bodySpawner,
                    _launchData,
                    _watermark
                );
            }
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        #endregion

        #region LayoutEditor

        public void SwitchViewMode() {
            _replayerUIPanel.SwitchViewMode();
        }

        #endregion
    }
}