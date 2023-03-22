using BeatLeader.Models;

namespace BeatLeader
{
    internal class FloatingConfig : SerializableSingleton<FloatingConfig>
    {
        public static readonly SerializablePose DefaultPose = 
            new(new(0, 1, 2), UnityEngine.Quaternion.Euler(new(40, 0, 0)));
        
        public SerializablePose Pose { get; set; } = DefaultPose;
        public float GridPosIncrement { get; set; } = 0.2f;
        public float GridRotIncrement { get; set; } = 5f;
        public bool IsPinned { get; set; } = true;
    }
}
