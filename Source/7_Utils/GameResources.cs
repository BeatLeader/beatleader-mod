using System.Linq;
using UnityEngine;

namespace BeatLeader {
    internal static class GameResources {
        public static readonly Material UINoGlowMaterial = Resources
            .FindObjectsOfTypeAll<Material>()
            .First(static x => x.name == "UINoGlow");

        public static readonly Material UIFontMaterial = Resources
            .FindObjectsOfTypeAll<Material>()
            .Where(static x => x.name == "Teko-Medium SDF Curved Softer")
            .First(static x => x.mainTexture.name == "Teko-Medium SDF Atlas");
    }
}