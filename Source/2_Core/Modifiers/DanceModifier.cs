using System.Linq;
using ModifiersCore;
using UnityEngine;

namespace BeatLeader {
    public static class DanceModifier {
        internal const string Id = "DNC";

        public static CustomModifier DNC = new CustomModifier(
            Id,
            "Dance Mode",
            "Score is how much you dance. Energy also drains when you stop dancing for a while",
            BundleLoader.AvatarIcon,
            new Color(0.6f, 0.2f, 0.9f),
            Color.yellow,
            0f,
            new string[] { ModifierCategories.Energy },
            new string[] { ModifierCategories.Energy },
            MutuallyExclusiveModifiers: NegativeModifiers.AlwaysNegative.Append(GolfModifier.Id).ToArray()
        );
    }
}
