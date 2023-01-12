using BeatLeader.Models;
using UnityEngine;

namespace BeatLeader {
    internal class FlyingViewConfig : SerializableSingleton<FlyingViewConfig> {
        public int FlySpeed { get; set; } = 4;
        public SerializableVector2 Sensitivity { get; set; } = new Vector2(0.5f, 0.5f);
    }
}
