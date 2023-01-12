using HarmonyLib;
using System.Reflection;

namespace BeatLeader {
    internal static class HarmonyUtils {
        public static void Patch(this Harmony harmony, HarmonyPatchDescriptor descriptor) {
            harmony.Patch(descriptor.Original, descriptor.Prefix, descriptor.Postfix);
        }

        public static void Unpatch(this Harmony harmony, HarmonyPatchDescriptor descriptor) {
            if (descriptor.Prefix != null)
                harmony.Unpatch(descriptor.Prefix.method, HarmonyPatchType.Prefix);
            if (descriptor.Postfix != null)
                harmony.Unpatch(descriptor.Postfix.method, HarmonyPatchType.Postfix);
        }
    }

    internal class HarmonyPatchDescriptor {
        public readonly MethodBase Original;
        public readonly HarmonyMethod? Prefix;
        public readonly HarmonyMethod? Postfix;

        public HarmonyPatchDescriptor(
            MethodBase original,
            MethodInfo? prefix = null,
            MethodInfo? postfix = null
        ) {
            Original = original;
            Prefix = prefix == null ? null : new HarmonyMethod(prefix);
            Postfix = postfix == null ? null : new HarmonyMethod(postfix);
        }
    }
}
