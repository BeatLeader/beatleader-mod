using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
using BeatLeader.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Models {
    [PublicAPI]
    public class LayoutEditorSettings {
        public SerializableVector2 ReferenceResolution { get; set; }
        public Dictionary<string, LayoutData> ComponentData { get; set; } = new();

        public void Migrate(Vector2 resolution, float scaleFactor) {
            if (ReferenceResolution == resolution) {
                return;
            }
            
            var delta = (resolution - ReferenceResolution) / 2f / scaleFactor;
            foreach (var pair in ComponentData.ToArray()) {
                var data = pair.Value;
                data.position += delta * Sign(data.position);
                ComponentData[pair.Key] = data;
            }
            
            ReferenceResolution = resolution;
        }

        private static Vector2 Sign(Vector2 vec) {
            return new Vector2(SignOrZero(vec.x), SignOrZero(vec.y));
        }

        private static float SignOrZero(float val) {
            return val == 0 ? 0 : Mathf.Sign(val);
        }
    }
}