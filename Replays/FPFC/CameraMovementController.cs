using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Replays.FPFC
{
    public class CameraMovementController : MonoBehaviour
    {
        protected bool _movementEnabled;

        public void Start()
        {
            if (_movementEnabled)
            {

            }
        }

        public void EnableMovement() => _movementEnabled = true;
        public void DisableMovement() => _movementEnabled = false;
    }
}
