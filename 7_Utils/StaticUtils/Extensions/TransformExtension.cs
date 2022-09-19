using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class TransformExtension
    {
        public static List<Transform> GetChildren(this Transform transform)
        {
            List<Transform> children = new List<Transform>();
            for(int idx = 0; idx < transform.childCount; idx++)
            {
                children.Add(transform.GetChild(idx));
            }
            return children;
        }
        public static void SetPose(this Transform transform, Pose pose)
        {
            transform.position = pose.position;
            transform.rotation = pose.rotation;
        }
        public static void SetPose(this Models.Transform transform, Pose pose)
        {
            transform.position = pose.position;
            transform.rotation = pose.rotation;
        }
        public static void SetLocalPose(this Transform transform, Pose pose)
        {
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;
        }
        public static Pose GetPose(this Transform transform)
        {
            return new Pose(transform.position, transform.rotation);
        }
        public static Pose GetPose(this Models.Transform transform)
        {
            return new Pose(transform.position, transform.rotation);
        }
        public static Pose GetLocalPose(this Transform transform)
        {
            return new Pose(transform.localPosition, transform.localRotation);
        }
        public static Pose Lerp(this Pose a, Pose b, float f)
        {
            var pose = new Pose();
            pose.position = Vector3.Lerp(a.position, b.position, f);
            pose.rotation = Quaternion.Lerp(a.rotation, b.rotation, f);
            return pose;
        }
    }
}
