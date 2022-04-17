using ReplayVector3 = BeatLeader.Models.Vector3;
using ReplayQuaternion = BeatLeader.Models.Quaternion;
using ReplayTransform = BeatLeader.Models.Transform;
using Vector3 = UnityEngine.Vector3;
using Quaternion = UnityEngine.Quaternion;
using Transform = UnityEngine.Transform;

namespace BeatLeader.Replays.Emulators
{
    public class FakeVRController : VRController
    {
        public override void Update() { }
        public void SetTransform(Vector3 pos, Quaternion rot)
        {
            _lastTrackedPosition = pos;
            transform.localPosition = pos;
            transform.localRotation = rot;
        }
        public void SetTransform(ReplayVector3 pos, ReplayQuaternion rot)
        {
            _lastTrackedPosition = pos;
            transform.localPosition = pos;
            transform.localRotation = rot;
        }
        public void SetTransform(Transform transform)
        {
            _lastTrackedPosition = transform.localPosition;
            this.transform.localPosition = transform.localPosition;
            this.transform.localRotation = transform.localRotation;
        }
        public void SetTransform(ReplayTransform transform)
        {
            _lastTrackedPosition = transform.position;
            this.transform.position = transform.position;
            this.transform.rotation = transform.rotation;
        }
    }
}
