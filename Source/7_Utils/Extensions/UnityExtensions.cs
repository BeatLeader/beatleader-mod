using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader.Utils {
    public static class UnityExtensions {
        public static T GetOrAddComponent<T>(this GameObject go) where T : Component {
            if (!go.TryGetComponent(out T component)) component = go.AddComponent<T>();
            return component;
        }

        public static bool TryDestroy(this Object obj) {
            if (!obj) return false;
            Object.Destroy(obj);
            return true;
        }

        public static IEnumerable<Transform> GetChildren(this Transform transform, bool firstLevel = true) {
            return GetChildren<Transform>(transform, firstLevel);
        }
        
        public static IEnumerable<T> GetChildren<T>(this Transform transform, bool firstLevel = true) where T : Component {
            return transform.GetComponentsInChildren<T>(true).Where(
                x => x.transform is {} t && t != transform && (!firstLevel || t.parent == transform));
        }

        public static void SetLocalPose(this Transform transform, Pose pose) {
            transform.localPosition = pose.position;
            transform.localRotation = pose.rotation;
        }

        public static void SetLocalPose(this Transform transform, SerializablePose pose) {
            SetLocalPose(transform, (Pose)pose);
        }

        public static Pose GetLocalPose(this Transform transform) {
            return new(transform.localPosition, transform.localRotation);
        }

        public static SerializablePose Lerp(this SerializablePose a, SerializablePose b, float f) {
            return new() {
                position = Vector3.Lerp(a.position, b.position, f),
                rotation = Quaternion.Lerp(a.rotation, b.rotation, f)
            };
        }
    }
}