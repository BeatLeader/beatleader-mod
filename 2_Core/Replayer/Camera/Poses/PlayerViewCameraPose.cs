using System.Linq;
using static BeatLeader.Replayer.Managers.InputManager;
using BeatLeader.Replayer.Movement;
using ICameraPoseProvider = BeatLeader.Models.ICameraPoseProvider;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Poses
{
    public class PlayerViewCameraPose : ICameraPoseProvider
    {
        public PlayerViewCameraPose(float smoothness, string name = "PlayerView")
        {
            _smoothness = smoothness;
            _name = name;
        }

        [Inject] private readonly VRControllersManager _vrControllersManager;

        private float _smoothness;
        private string _name;

        public InputType[] AvailableInputs => new[] { InputType.FPFC };
        public bool SupportsOffset => true;
        public bool UpdateEveryFrame => true;
        public string Name => _name;

        public Pose GetPose(Pose cameraPose)
        {
            Vector3 position = Vector3.Lerp(cameraPose.position, _vrControllersManager.Head.transform.position, Time.deltaTime * _smoothness);
            Quaternion rotation = Quaternion.Lerp(cameraPose.rotation, _vrControllersManager.Head.transform.rotation, Time.deltaTime * _smoothness);
            return new Pose(position, rotation);
        }
    }
}
