using System;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BeatLeader {
    internal static class GameResources {
        public static class Sprites {
            public static readonly Sprite SearchIcon = GetResource<Sprite>("SearchIcon");

            public static readonly Sprite FilterIcon = GetResource<Sprite>("FilterIcon");
            
            public static readonly Sprite EditIcon = GetResource<Sprite>("EditIcon");
            
            public static readonly Sprite ArrowIcon = GetResource<Sprite>("ArrowIcon");
            
            public static readonly Sprite Caret = GetResource<Sprite>("Caret");

            public static readonly Sprite VerticalRoundRect = GetResource<Sprite>("VerticalRoundRect8");
            
            public static readonly Sprite RoundRect = GetResource<Sprite>("RoundRect10");
            
            public static readonly Sprite Circle = GetResource<Sprite>("FullCircle64");
        }

        public static readonly Material UINoGlowAdditiveMaterial = Resources
            .FindObjectsOfTypeAll<Material>()
            .First(static x => x.name == "UINoGlowAdditive");

        public static readonly Material UINoGlowMaterial = Resources
            .FindObjectsOfTypeAll<Material>()
            .First(static x => x.name == "UINoGlow");

        public static readonly Material UIFogBackgroundMaterial = Resources
            .FindObjectsOfTypeAll<Material>()
            .First(static x => x.name == "UIFogBG");

        public static readonly Material UIFontMaterial = Resources
            .FindObjectsOfTypeAll<Material>()
            .Where(static x => x.name == "Teko-Medium SDF Curved Softer")
            .First(static x => x.mainTexture.name == "Teko-Medium SDF Atlas");

        public static readonly Signal ClickSignal = Resources
            .FindObjectsOfTypeAll<Signal>()
            .First(static x => x.name == "TableCellWasPressed");

        [Obsolete("This field is obsolete, use Sprites.RoundRect instead")]
        public static readonly Sprite RoundRectSprite = Resources
            .FindObjectsOfTypeAll<Sprite>()
            .First(static x => x.name == "RoundRect10");

        private static T GetResource<T>(string name) where T : Object {
            return GetResource<T>(x => x.name == name);
        }

        private static T GetResource<T>(Func<T, bool> predicate) where T : Object {
            return Resources.FindObjectsOfTypeAll<T>().First(predicate);
        }
    }
}