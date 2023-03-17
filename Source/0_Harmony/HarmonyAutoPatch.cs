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
            harmony.Unpatch(descriptor);
        }

        public static implicit operator HarmonyAutoPatch(HarmonyPatchDescriptor descriptor) => new(descriptor);
    }
}
