using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine.Scripting;

namespace BeatLeader.Utils {
    //thanks Kinsi https://github.com/kinsi55/BeatSaber_Shaffuru/blob/master/GameLogic/RamCleaner.cs
    [PublicAPI]
    public static class MemoryManager {
        public static int MemoryThreshold = 2048;
        public static int CleaningInterval = 60;

        private static CancellationTokenSource? _autoCheckTaskTokenSource;
        private static DateTime _availableToCheckDateTime = DateTime.Now;
        private static bool _enabledAutomaticCheck;

        public static async Task CleanMemory(int iterations = 8, int iterationThreshold = 200, CancellationToken token = default) {
            var oldMode = GarbageCollector.GCMode;
            for (var i = 1; i <= iterations; i++) {
                await Task.Delay(iterationThreshold, token);
                if (token.IsCancellationRequested) return;
                GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
                GC.Collect();
                if (i != iterations) continue;
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            GarbageCollector.GCMode = oldMode;
        }

        public static async Task CleanIfNeeded(CancellationToken token = default) {
            if (CleaningInterval != -1 && DateTime.Now < _availableToCheckDateTime || !CleanRequired()) return;
            _availableToCheckDateTime = DateTime.Now.AddSeconds(CleaningInterval);
            await CleanMemory(token: token);
        }

        public static bool CleanRequired() {
            return GC.GetTotalMemory(false) >= MemoryThreshold * 1024L * 1024L;
        }
    }
}