using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Models
{
    public struct CombinedCameraMovementData
    {
        public CombinedCameraMovementData(UnityEngine.Transform camera, UnityEngine.Transform head, UnityEngine.Transform origin)
        {
            cameraPose = new Pose(camera.localPosition, camera.localRotation);
            headPose = new Pose(head.localPosition, head.localRotation);
            originPose = new Pose(origin.position, origin.rotation);
        }

        public Pose cameraPose;
        public Pose headPose;
        public readonly Pose originPose;
    }
}
