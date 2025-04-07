using System;
using System.IO;
using IPA.Utilities;

namespace BeatLeader.Replayer {
    [Obsolete("Use ReplayManager instead")]
    internal static class ReplayerCache {
        public static readonly string CacheDirectory = Path.Combine(UnityGame.UserDataPath, "BeatLeader", "ReplayerCache\\");
    }
}