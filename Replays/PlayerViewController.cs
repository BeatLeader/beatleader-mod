using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPA.Utilities;
using BeatLeader.Replays.Movement;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class PlayerViewController : MonoBehaviour
    {
        [Inject] protected readonly MovementManager _movementManager;

        protected Camera _camera;
        protected float _rotationSmooth = 2;
        protected float _positionSmooth = 2;
        protected bool _initialized;

        public void Start()
        {
            _camera = Resources.FindObjectsOfTypeAll<SmoothCamera>().First(x => x.transform.parent.name == "LocalPlayerGameCore").GetField<Camera, SmoothCamera>("_camera");
            _camera.fieldOfView = 120;
            _camera.enabled = true;
            _initialized = true;
        }
        public void LateUpdate()
        {
            if (_movementManager.initialized && _initialized)
            {
                Vector3 position = Vector3.Lerp(_camera.transform.position, _movementManager.head.position, Time.deltaTime * _positionSmooth);
                Quaternion rotation = Quaternion.Slerp(_camera.transform.rotation, _movementManager.head.rotation, Time.deltaTime * _rotationSmooth);
                _camera.transform.SetPositionAndRotation(position, rotation);
            }
        }
    }
}
