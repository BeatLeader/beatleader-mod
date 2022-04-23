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
        [Inject] protected readonly MainCamera _mainCamera;

        protected Camera _camera;
        protected float _rotationSmooth = 4;
        protected float _positionSmooth = 4;

        public void Start()
        {
            _camera = new GameObject("ReplayCamera").AddComponent<Camera>();
            _camera.fieldOfView = 120;
            //_mainCamera.transform.SetParent(_movementManager.head.transform);
        }
        public void LateUpdate()
        {
            Vector3 position = Vector3.Lerp(_camera.transform.position, _movementManager.head.position, Time.deltaTime * _positionSmooth);
            Quaternion rotation2 = Quaternion.Slerp(_camera.transform.rotation, _movementManager.head.rotation, Time.deltaTime * _rotationSmooth);
            _camera.transform.SetPositionAndRotation(position, rotation2);
        }
    }
}
