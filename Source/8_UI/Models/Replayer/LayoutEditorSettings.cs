using System.Collections.Generic;
using System.Linq;
using BeatLeader.Components;
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
            for (var i = 0; i < 2; i++) {
                delta[i] *= resolution[i] > ReferenceResolution[i] ? 1 : -1;
            }
            //
            foreach (var pair in ComponentData.ToArray()) {
                var data = pair.Value;
                if (data.migrationRules is LayoutMigrationRules.None) {
                    continue;
                }
                data.position += delta * Sign(data.position) * ZeroedRules(data.migrationRules);
                ComponentData[pair.Key] = data;
            }
            ReferenceResolution = resolution;
        }

        private static Vector2 ZeroedRules(LayoutMigrationRules rules) {
            return new Vector2(
                rules is LayoutMigrationRules.X ? 1 : 0,
                rules is LayoutMigrationRules.Y ? 1 : 0
            );
        }

        private static Vector2 Sign(Vector2 vec) {
            return new Vector2(Mathf.Sign(vec.x), Mathf.Sign(vec.y));
        }
    }
}