using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    public class PlayerViewCameraView : CameraView {
        [Inject] private readonly IVirtualPlayerBodySpawner _bodySpawner = null!;

        public static float SMOOTHNESS_MAX => 10;

        public override string Name { get; init; } = "PlayerView";

        public SerializableVector3 PositionOffset { get; set; } = new(0, 0, -1);
        public float Smoothness { get; set; }
        public bool KeepUpright { get; set; }
        public bool KeepStraight { get; set; }
        
        private Vector3 _lastHeadPos;
        private Quaternion _lastHeadRot = Quaternion.identity;

        public override Pose ProcessPose(Pose headPose) {
            var rotation = headPose.rotation;
            var position = headPose.position + rotation * new Vector3(0, 0, PositionOffset.z);
            if (KeepUpright) {
                rotation.z = 0f;
                rotation.Normalize();
            }
            if (KeepStraight) {
                rotation.x = 0f;
                rotation.Normalize();
            }

            if (Smoothness > 0 && Smoothness <= SMOOTHNESS_MAX) {
                var f = Time.deltaTime * (SMOOTHNESS_MAX - Smoothness + 1);
                position = Vector3.Lerp(_lastHeadPos, position, f);
                rotation = Quaternion.Lerp(_lastHeadRot, rotation, f);
            }

            _lastHeadPos = position;
            _lastHeadRot = rotation;

            return new(position, rotation);
        }

        public override void OnEnable() {
            _bodySpawner.BodyHeadsVisible = false;
        }

        public override void OnDisable() {
            _bodySpawner.BodyHeadsVisible = true;
        }
    }
}