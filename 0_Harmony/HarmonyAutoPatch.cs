using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BeatLeader
{
    internal class HarmonyAutoPatch : IDisposable
    {
        public HarmonyAutoPatch(HarmonyPatchDescriptor descriptor)
        {
            Descriptor = descriptor;
            harmony.Patch(descriptor);
        }

        public HarmonyPatchDescriptor Descriptor { get; private set; }

        public void Dispose()
        {
            if (Descriptor.Prefix != null)
                harmony.Unpatch(Descriptor.Prefix.method, HarmonyPatchType.Prefix);
            if (Descriptor.Postfix != null)
                harmony.Unpatch(Descriptor.Postfix.method, HarmonyPatchType.Postfix);
        }

        private static readonly Harmony harmony = new("BeatLeader.AutoPatchesRegistry");
    }
}
