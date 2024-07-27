using BeatLeader.Models;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace BeatLeader.Replayer {
    public sealed class StaticCameraView : CameraView {
        [JsonConstructor, UsedImplicitly]
        private StaticCameraView() { }

        public StaticCameraView(string name, Vector3 position, Vector3 rotation) {
            Name = name;
            _rotation = Quaternion.Euler(rotation);
            _position = position;
        }

        public override string Name { get; init; } = null!;

        [JsonProperty("Rotation")]
        private readonly SerializableQuaternion _rotation;

        [JsonProperty("Position")]
        private readonly SerializableVector3 _position;

        public override Pose ProcessPose(Pose headPose) {
            return new(_position, _rotation);
        }
    }
}