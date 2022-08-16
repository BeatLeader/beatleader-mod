using Zenject;

namespace BeatLeader.Replayer
{
    public class GameSettingsLoader : IInitializable
    {
        [InjectOptional] private readonly PlayerDataModel _playerDataModel;
        [InjectOptional] private readonly ReplayerCameraController _cameraController;

        public virtual void Initialize()
        {
            if (_playerDataModel == null || _cameraController == null) return;
            if (!_playerDataModel.playerData.playerSpecificSettings.reduceDebris)
                _cameraController.CullingMask |= 1 << LayerMasks.noteDebrisLayer;
            else
                _cameraController.CullingMask &= ~(1 << LayerMasks.noteDebrisLayer);
        }
    }
}
