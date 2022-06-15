using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BeatLeader.Replays.Managers.InputManager;
using BeatLeader.Replays.Movement;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlayerViewCameraPose : ICameraPoseProvider
    {
        public PlayerViewCameraPose(float smoothness, string name = "PlayerView")
        {
            _smoothness = smoothness;
            _name = name;
        }

        [Inject] protected readonly VRControllersManager _vrControllersManager;

        protected float _smoothness;
        protected string _name;

        public InputSystemType[] availableSystems => new InputSystemType[] { InputSystemType.FPFC };
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
