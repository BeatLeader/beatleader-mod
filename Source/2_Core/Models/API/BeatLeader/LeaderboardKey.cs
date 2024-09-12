﻿namespace BeatLeader.Models {
    internal readonly struct LeaderboardKey {
        public readonly string Hash;
        public readonly string Diff;
        public readonly string Mode;

        #region Properties

        public int DiffId => FormatUtils.DiffIdForDiffName(Diff);

        #endregion

        #region Constructors

        public LeaderboardKey(string hash, string diff, string mode) {
            Hash = hash.ToUpper();
            Diff = diff;
            Mode = mode;
        }

        public static LeaderboardKey FromSongDiff(SongInfo song, DiffInfo diff) {
            return new LeaderboardKey(
                song.hash,
                diff.difficultyName,
                diff.modeName
            );
        }

        public static LeaderboardKey FromBeatmap(BeatmapKey beatmapKey) {
            var hash = beatmapKey.levelId.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");
            if (hash.Length > 40) {
                hash = hash.Substring(0, 40);
            }
            var diff = beatmapKey.difficulty.ToString();
            var mode = beatmapKey.beatmapCharacteristic.serializedName;
            return new LeaderboardKey(hash, diff, mode);
        }

        #endregion

        #region Equals/HashCode

        public bool Equals(LeaderboardKey other) => Hash == other.Hash && Diff == other.Diff && Mode == other.Mode;

        public override bool Equals(object obj) => obj is LeaderboardKey other && Equals(other);

        public override int GetHashCode() => (Hash, Diff, Mode).GetHashCode();

        #endregion
    }
}