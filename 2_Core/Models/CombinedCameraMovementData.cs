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
            CameraPose = new Pose(camera.localPosition, camera.localRotation);
            HeadPose = new Pose(head.localPosition, head.localRotation);
            OriginPose = new Pose(origin.position, origin.rotation);
        }

        public Pose CameraPose;
        public Pose HeadPose;
        public Pose OriginPose { get; }
    }
}
