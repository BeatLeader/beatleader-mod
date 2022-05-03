using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Replays
{
    public class MainCameraBlocker : MonoBehaviour //FUCK YOU, AUROS! WHY NOT TO ALLOW PEOPLE TO DISABLE FUCKING CURSOR FOLLOWING?
    {
        protected Quaternion rotation;
        protected Vector3 position;
        protected bool onLock; //I am not like auros

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
        public void Unlock() //AUROS, WHY NOT???????
        {
            rotation = Quaternion.identity;
            position = Vector3.zero;
            onLock = false;
        }
    }
}
