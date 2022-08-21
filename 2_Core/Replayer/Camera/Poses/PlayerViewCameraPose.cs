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
            this.smoothness = smoothness;
            _name = name;
        }

        [Inject] private readonly VRControllersManager _vrControllersManager;

        public InputType AvailableInputs => InputType.FPFC;
        public int Id => 4;
        public bool UpdateEveryFrame => true;
        public string Name => _name;

        public Vector3 offset;
        public float smoothness;

        private string _name;

        public Pose GetPose(Pose cameraPose)
        {
            cameraPose.position -= offset;
            Vector3 position = Vector3.Lerp(cameraPose.position, _vrControllersManager.Head.transform.position, Time.deltaTime * smoothness);
            Quaternion rotation = Quaternion.Lerp(cameraPose.rotation, _vrControllersManager.Head.transform.rotation, Time.deltaTime * smoothness);
            position += offset;
            return new Pose(position, rotation);
        }
    }
}
