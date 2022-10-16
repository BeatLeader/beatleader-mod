using HarmonyLib;
using System;

namespace BeatLeader
{
    internal class HarmonyAutoPatch : IDisposable
    {
        public HarmonyAutoPatch(HarmonyPatchDescriptor descriptor)
        {
            Descriptor = descriptor;
            _harmony.Patch(descriptor);
        }

        public HarmonyPatchDescriptor Descriptor { get; private set; }

        public void Dispose()
        {
            if (Descriptor.Prefix != null)
                _harmony.Unpatch(Descriptor.Prefix.method, HarmonyPatchType.Prefix);
            if (Descriptor.Postfix != null)
                _harmony.Unpatch(Descriptor.Postfix.method, HarmonyPatchType.Postfix);
        }

        private static readonly Harmony _harmony = new("BeatLeader.AutoPatchesRegistry");
    }
}
