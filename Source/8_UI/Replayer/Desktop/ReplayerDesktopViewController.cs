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
        [Inject] private readonly IBodySettingsViewFactory _bodySettingsFactory = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;
        [Inject] private readonly ICameraController _cameraController = null!;
        [Inject] private readonly IReplayWatermark _watermark = null!;

        #endregion

        #region Setup

        private ReplayerUIPanel _replayerUIPanel = null!;

        private void Awake() {
            gameObject.layer = 5;
            gameObject.AddComponent<GraphicRaycaster>();
            Destroy(gameObject.GetComponent<BaseRaycaster>());
            
            _replayerUIPanel = new ReplayerUIPanel();
            _replayerUIPanel.WithRectExpand().Use(transform);
            _replayerUIPanel.Setup(
                _pauseController,
                _finishController,
                _timeController,
                _playersManager,
                _cameraController,
                _bodySettingsFactory,
                _launchData,
                _watermark
            );
        }
        
        #endregion

        #region LayoutEditor

        public void SwitchViewMode() {
            _replayerUIPanel.SwitchViewMode();
        }

        #endregion
    }
}