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
            harmony.Patch(descriptor.Original, autoPatchPrefix, autoPatchPostfix);
            descriptors.Add(descriptor);
        }

        public HarmonyPatchDescriptor Descriptor { get; private set; }

        public void Dispose()
        {
            descriptors.Remove(Descriptor);
            if (Descriptor.Prefix != null)
                harmony.Unpatch(Descriptor.Prefix.method, HarmonyPatchType.Prefix);
            if (Descriptor.Postfix != null)
                harmony.Unpatch(Descriptor.Postfix.method, HarmonyPatchType.Postfix);
        }

        private static readonly Harmony harmony = new("BeatLeader.AutoPatchesRegistry");
        private static readonly HashSet<HarmonyPatchDescriptor> descriptors = new();

        private static readonly HarmonyMethod autoPatchPrefix =
            new(typeof(HarmonyAutoPatch).GetMethod(nameof(AutoPatchPrefix),
                BindingFlags.Static | BindingFlags.NonPublic));

        private static readonly HarmonyMethod autoPatchPostfix =
            new(typeof(HarmonyAutoPatch).GetMethod(nameof(AutoPatchPostfix),
                BindingFlags.Static | BindingFlags.NonPublic));

        private static void AutoPatchPrefix(MethodInfo __originalMethod, object __instance)
        {
            InvokeMethod(HarmonyPatchType.Prefix, __originalMethod, __instance);
        }
        private static void AutoPatchPostfix(MethodInfo __originalMethod, object __instance)
        {
            InvokeMethod(HarmonyPatchType.Postfix, __originalMethod, __instance);
        }

        private static void InvokeMethod(HarmonyPatchType type, MethodInfo original, object instance)
        {
            foreach (var descriptor in descriptors)
            {
                var originalMethod = descriptor.Original;
                if (originalMethod != original) continue;

                var invokationMethod = type switch
                {
                    HarmonyPatchType.Prefix => descriptor.Prefix?.method,
                    HarmonyPatchType.Postfix => descriptor.Postfix?.method,
                    _ => null
                };
                if (invokationMethod == null) continue;

                var resolvedParams = invokationMethod.GetParameters();
                var invokationParams = new object[resolvedParams.Length];

                if (invokationParams.Length > 0)
                {
                    for (int i = 0; i < resolvedParams.Length; i++)
                    {
                        var param = resolvedParams[i];
                        invokationParams[i] = param.Name == "__instance"
                            && param.ParameterType == instance.GetType() ? instance : null;
                    }
                }

                invokationMethod.Invoke(null, invokationParams);
            }
        }
    }
}
