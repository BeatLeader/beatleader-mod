using System.Linq;
using ModifiersCore;
using UnityEngine;

namespace BeatLeader {
    public static class GolfModifier {
        internal const string Id = "GOLF";

        public static CustomModifier GOLF = new CustomModifier(
            Id,
            "Golf",
            "Score goes only to the Golf leaderboard, where lower accuracy is better. Needs accuracy of at most 50%",
            BundleLoader.GolfIcon,
            new Color(0.2f, 0.75f, 0.3f),
            Color.yellow,
            0f,
            MutuallyExclusiveModifiers: NegativeModifiers.AlwaysNegative.Append(DanceModifier.Id).ToArray()
        );
    }
}
