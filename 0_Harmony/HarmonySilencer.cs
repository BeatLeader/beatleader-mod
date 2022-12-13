using BeatLeader.Utils;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace BeatLeader {
    internal class HarmonySilencer : IDisposable {
        public HarmonySilencer(MethodInfo method, bool enable = true) {
            this.method = method;
            harmony.Patch(method, silencerPrefix);
            silencersRegistry.Add(method, enable);
        }

        private static readonly Harmony harmony = new("BeatLeader.SilencersRegistry");
        private static readonly Dictionary<MethodInfo, bool> silencersRegistry = new();

        private static readonly HarmonyMethod silencerPrefix =
            new(typeof(HarmonySilencer).GetMethod(nameof(SilencerPrefix), ReflectionUtils.StaticFlags));

        public bool Enabled {
            get => silencersRegistry[method];
            set => silencersRegistry[method] = value;
        }

        public readonly MethodInfo method;

        public void Dispose() {
            harmony.Unpatch(method, HarmonyPatchType.Prefix);
            silencersRegistry.Remove(method);
        }

        private static bool SilencerPrefix(MethodInfo __originalMethod) {
            return !(silencersRegistry.TryGetValue(__originalMethod, out var result) && result);
        }
    }
}
