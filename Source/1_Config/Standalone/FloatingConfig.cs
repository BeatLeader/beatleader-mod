using BeatLeader.Models;
using BeatLeader.Models.Replay;

namespace BeatLeader
{
    internal class FloatingConfig : SerializableSingleton<FloatingConfig>
    {
        public static readonly UnityEngine.Pose DefaultPose = 
            new(new(0, 1, 2), UnityEngine.Quaternion.Euler(new(40, 0, 0)));

        public UnityEngine.Pose Pose {
            get => new(Position, Rotation); 
            set {
                Position = value.position;
                Rotation = value.rotation;
            }
        }
        public Vector3 Position { get; set; } = DefaultPose.position;
        public Quaternion Rotation { get; set; } = DefaultPose.rotation;
        public float GridPosIncrement { get; set; } = 0.2f;
        public float GridRotIncrement { get; set; } = 5f;
        public bool IsPinned { get; set; } = true;
    }
}
