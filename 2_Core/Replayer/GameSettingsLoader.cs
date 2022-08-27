using BeatLeader.Models;
using BeatLeader.Replayer.Camera;
using Zenject;

namespace BeatLeader.Replayer
{
    public class GameSettingsLoader : IInitializable
    {
        [InjectOptional] private readonly PlayerDataModel _playerDataModel;
        [InjectOptional] private readonly ReplayerCameraController _cameraController;
        [InjectOptional] private readonly ReplayLaunchData _replayData;

        public virtual void Initialize()
        {
            if (_playerDataModel == null || _cameraController == null) return;

            bool showDebris = !_playerDataModel.playerData.playerSpecificSettings.reduceDebris;
            showDebris = _replayData != null && _replayData.overrideSettings ? _replayData.settings.showDebris : showDebris;

            if (showDebris)
                _cameraController.CullingMask |= 1 << LayerMasks.noteDebrisLayer;
            else
                _cameraController.CullingMask &= ~(1 << LayerMasks.noteDebrisLayer);
        }
    }
}
