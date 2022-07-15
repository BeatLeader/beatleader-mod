using System.Linq;
using static BeatLeader.Replays.Managers.InputManager;
using BeatLeader.Replays.Movement;
using ICameraPoseProvider = BeatLeader.Models.ICameraPoseProvider;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays.Poses
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

        public InputType[] availableInputs => new[] { InputType.FPFC };
        public bool selfInject => true;
        public bool updateEveryFrame => true;
        public string name => _name;

        public Pose GetPose(Pose cameraPose)
        {
            Vector3 position = Vector3.Lerp(cameraPose.position, _vrControllersManager.head.transform.position, Time.deltaTime * _smoothness);
            Quaternion rotation = Quaternion.Lerp(cameraPose.rotation, _vrControllersManager.head.transform.rotation, Time.deltaTime * _smoothness);
            return new Pose(position, rotation);
        }
    }
}
