using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BeatLeader.Replays.Managers.InputManager;
using UnityEngine;

namespace BeatLeader.Replays
{
    public struct StaticCameraPose : ICameraPoseProvider
    {
        public StaticCameraPose()
        {
            _rotation = Quaternion.identity;
            _position = Vector3.zero;
            _name = "NaN";
            _availableSystems = new InputSystemType[] { InputSystemType.FPFC, InputSystemType.VR };
        }
        public StaticCameraPose(string name, Vector3 position, Quaternion rotation)
        {
            _rotation = rotation;
            _position = position;
            _name = name;
            _availableSystems = new InputSystemType[] { InputSystemType.FPFC, InputSystemType.VR };
        }
        public StaticCameraPose(string name, Vector3 position, Quaternion rotation, params InputSystemType[] availableSystems)
        {
            _rotation = rotation;
            _position = position;
            _name = name;
            _availableSystems = availableSystems;
        }

        private InputSystemType[] _availableSystems;
        private Quaternion _rotation;
        private Vector3 _position;
        private string _name;

        public InputSystemType[] availableSystems => _availableSystems;
        public bool injectAutomatically => false;
        public bool updateEveryFrame => false;
        public string name => _name;

        public Pose GetPose(Pose cameraPose) => new Pose(_position, _rotation);
    }
}
