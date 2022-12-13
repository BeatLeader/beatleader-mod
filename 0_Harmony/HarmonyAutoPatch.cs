using HarmonyLib;
using System;

namespace BeatLeader {
    internal class HarmonyAutoPatch : IDisposable {
        public HarmonyAutoPatch(HarmonyPatchDescriptor descriptor) {
            this.descriptor = descriptor;
            harmony.Patch(descriptor);
        }

        private static readonly Harmony harmony = new("BeatLeader.AutoPatchesRegistry");

        public readonly HarmonyPatchDescriptor descriptor;

        public void Dispose() {
            if (descriptor.Prefix != null)
                harmony.Unpatch(descriptor.Prefix.method, HarmonyPatchType.Prefix);
            if (descriptor.Postfix != null)
                harmony.Unpatch(descriptor.Postfix.method, HarmonyPatchType.Postfix);
        }

        public static implicit operator HarmonyAutoPatch(HarmonyPatchDescriptor descriptor) => new(descriptor);
    }
}
