using System;
using System.IO;
using BeatLeader.Models.Replay;
using BeatLeader.Utils;
using IPA.Utilities;

namespace BeatLeader.Replayer {
    [Obsolete("Use ReplayManager instead")]
    internal static class ReplayerCache {
        public static readonly string CacheDirectory = Path.Combine(UnityGame.UserDataPath, "BeatLeader", "ReplayerCache\\");

        private static string GetFileName(int scoreId) => Path.Combine(CacheDirectory, $"{scoreId}.bsor");

        public static bool TryReadReplay(int scoreId, out Replay replay) {
            var fileName = GetFileName(scoreId);
            return FileManager.TryReadReplay(fileName, out replay);
        }

        public static bool TryWriteReplay(int scoreId, Replay replay) {
            var fileName = GetFileName(scoreId);
            return FileManager.TryWriteReplay(fileName, replay);
        }
    }
}