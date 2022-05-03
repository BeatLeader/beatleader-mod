using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Replays
{
    public class MainCameraBlocker : MonoBehaviour
    {
        protected Quaternion rotation;
        protected Vector3 position;
        protected bool onLock;

        public void LateUpdate()
        {
            if (onLock)
            {
                transform.rotation = rotation;
                transform.position = position;
            }
        }
        public void Lock()
        {
            rotation = transform.rotation;
            position = transform.position;
            onLock = true;
        }
        public void Unlock()
        {
            rotation = Quaternion.identity;
            position = Vector3.zero;
            onLock = false;
        }
    }
}
