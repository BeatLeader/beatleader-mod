using Zenject;

namespace BeatLeader.Replays.Emulating
{
    public class GameSettingsLoader : IInitializable
    {
        [InjectOptional] protected readonly PlayerDataModel _playerDataModel;
        [InjectOptional] protected readonly ReplayerCameraController _cameraController;

        public void Initialize()
        {
            if (_playerDataModel == null || _cameraController == null) return;
            if (!_playerDataModel.playerData.playerSpecificSettings.reduceDebris)
                _cameraController.cullingMask |= 1 << LayerMasks.noteDebrisLayer;
            else
                _cameraController.cullingMask &= ~(1 << LayerMasks.noteDebrisLayer);
        }
    }
}
