using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    public class PlayerViewCameraView : CameraView {
        [Inject] private readonly IVirtualPlayerBodySpawner _bodySpawner = null!;

        public override string Name { get; init; } = "PlayerView";

        public SerializableVector3 PositionOffset { get; set; } = new(0, 0, -1);
        public SerializableQuaternion RotationOffset { get; set; } = Quaternion.identity;
        public float Smoothness { get; set; } = 8;
        
        private Vector3 _lastHeadPos;
        private Quaternion _lastHeadRot = Quaternion.identity;

        public override Pose ProcessPose(Pose headPose) {
            var position = headPose.position + PositionOffset;
            var rotation = headPose.rotation * RotationOffset;

            var f = Time.deltaTime * Smoothness;
            position = Vector3.Lerp(_lastHeadPos, position, f);
            rotation = Quaternion.Lerp(_lastHeadRot, rotation, f);

            _lastHeadPos = position;
            _lastHeadRot = rotation;

            return new(position, rotation);
        }

        public override void OnEnable() {
            _bodySpawner.BodyConfigs.Values.ForEach(static x => x.AnchorMask &= ~BodyNode.Head);
        }

        public override void OnDisable() {
            _bodySpawner.BodyConfigs.Values.ForEach(static x => x.AnchorMask |= BodyNode.Head);
        }
    }
}