using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer {
    public class PlayerViewCameraView : CameraView {
        [Inject] private readonly IVirtualPlayerBodySpawner _bodySpawner = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;

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
            if (KeepUpright || KeepStraight) {
                RefreshAlignment();
                var euler = rotation.eulerAngles;
                if (KeepUpright) euler.z -= _averageRoll;
                if (KeepStraight) euler.x -= _averagePitch;
                rotation = Quaternion.Euler(euler);
            }
            var position = headPose.position + rotation * new Vector3(0, 0, PositionOffset.z);

            if (Smoothness > 0 && Smoothness <= SMOOTHNESS_MAX) {
                var f = Time.deltaTime * (SMOOTHNESS_MAX - Smoothness + 1);
                position = Vector3.Lerp(_lastHeadPos, position, f);
                rotation = Quaternion.Lerp(_lastHeadRot, rotation, f);
            }

            _lastHeadPos = position;
            _lastHeadRot = rotation;

            return new(position, rotation);
        }

        #region Alignment

        private IReplay? _alignedReplay;
        private float _averagePitch;
        private float _averageRoll;

        private void RefreshAlignment() {
            var replay = _playersManager.PrimaryPlayer?.Replay;
            if (replay == _alignedReplay) return;
            _alignedReplay = replay;
            _averagePitch = _averageRoll = 0f;
            if (replay == null) return;

            var count = 0;
            foreach (var frame in replay.PlayerMovementFrames) {
                Quaternion rotation = frame.headPose.rotation;
                if (Mathf.Abs(Quaternion.Dot(rotation, rotation) - 1f) > 0.01f) continue;
                var euler = rotation.eulerAngles;
                _averagePitch += Mathf.DeltaAngle(0f, euler.x);
                _averageRoll += Mathf.DeltaAngle(0f, euler.z);
                count++;
            }
            if (count == 0) return;
            _averagePitch /= count;
            _averageRoll /= count;
        }

        #endregion

        public override void OnEnable() {
            _bodySpawner.BodyHeadsVisible = false;
        }

        public override void OnDisable() {
            _bodySpawner.BodyHeadsVisible = true;
        }
    }
}