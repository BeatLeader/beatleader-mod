using System.Reflection;
using HarmonyLib;
using JetBrains.Annotations;

namespace BeatLeader {
    internal static class HarmonyUtils {
        public static void Patch(this Harmony harmony, HarmonyPatchDescriptor patchDescriptor) {
            harmony.Patch(patchDescriptor.Original, patchDescriptor.Prefix, patchDescriptor.Postfix);
        }
    }

    internal class HarmonyPatchDescriptor {
        public readonly MethodInfo Original;
        [CanBeNull] public readonly HarmonyMethod Prefix;
        [CanBeNull] public readonly HarmonyMethod Postfix;

        public HarmonyPatchDescriptor(
            MethodInfo original,
            [CanBeNull] MethodInfo prefix = null,
            [CanBeNull] MethodInfo postfix = null
        ) {
            Original = original;
            Prefix = prefix == null ? null : new HarmonyMethod(prefix);
            Postfix = postfix == null ? null : new HarmonyMethod(postfix);
        }
    }
}