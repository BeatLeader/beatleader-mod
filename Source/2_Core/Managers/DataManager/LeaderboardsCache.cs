using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using Newtonsoft.Json;

namespace BeatLeader.DataManager {
    internal static class LeaderboardsCache {
        #region LeaderboardsCache

        private static readonly Dictionary<LeaderboardKey, LeaderboardCacheEntry> LeaderboardInfoCache = new();
        public static long LastCheckTime;

        public static bool TryGetLeaderboardInfo(LeaderboardKey key, out LeaderboardCacheEntry data) {
            if (!LeaderboardInfoCache.ContainsKey(key)) {
                data = new LeaderboardCacheEntry();
                return false;
            }

            data = LeaderboardInfoCache[key];
            return true;
        }

        public static void PutLeaderboardInfo(SongInfo songInfo, string leaderboardId, DiffInfo diffInfo, QualificationInfo qualificationInfo, Clan clan, bool clanRankingContested) {
            var key = LeaderboardKey.FromSongDiff(songInfo, diffInfo);
            LeaderboardInfoCache[key] = new LeaderboardCacheEntry(leaderboardId, songInfo, diffInfo, qualificationInfo, clan, clanRankingContested);
        }

        public readonly struct LeaderboardCacheEntry {
            public readonly string LeaderboardId;
            public readonly SongInfo SongInfo;
            public readonly DiffInfo DifficultyInfo;
            public readonly QualificationInfo QualificationInfo;
            public readonly Clan Clan;
            public readonly bool ClanRankingContested;

            public LeaderboardCacheEntry(string leaderboardId, SongInfo songInfo, DiffInfo difficultyInfo, QualificationInfo qualificationInfo, Clan clan, bool clanRankingContested) {
                SongInfo = songInfo;
                DifficultyInfo = difficultyInfo;
                QualificationInfo = qualificationInfo;
                LeaderboardId = leaderboardId;
                Clan = clan;
                ClanRankingContested = clanRankingContested;
            }
        }

        #endregion

        #region SortingCache

        private static readonly Dictionary<string, SortEntry> SortingCache = new();

        private static void RecalculateSortingCache() {
            SortingCache.Clear();

            foreach (var pair in LeaderboardInfoCache) {
                var hash = pair.Key.Hash;

                var sortEntry = SortingCache.ContainsKey(hash) ? SortingCache[hash] : new SortEntry();
                sortEntry.Update(pair.Value.DifficultyInfo);
                SortingCache[hash] = sortEntry;
            }
        }

        public static SortEntry? GetSortingInfo(string hash) {
            return SortingCache.ContainsKey(hash) ? SortingCache[hash] : null;
        }

        public class SortEntry {
            public float HighestStars;
            public float HighestTechStars;
            public float HighestAccStars;
            public float HighestPassStars;
            public bool IsNominated;
            public bool IsQualified;
            public bool IsRanked;

            public void Update(DiffInfo diffInfo) {
                if (HighestStars < diffInfo.stars) HighestStars = diffInfo.stars;
                if (HighestTechStars < diffInfo.techRating) HighestTechStars = diffInfo.techRating;
                if (HighestAccStars < diffInfo.accRating) HighestAccStars = diffInfo.accRating;
                if (HighestPassStars < diffInfo.passRating) HighestPassStars = diffInfo.passRating;

                var status = FormatUtils.GetRankedStatus(diffInfo);
                IsNominated |= status is RankedStatus.Nominated;
                IsQualified |= status is RankedStatus.Qualified;
                IsRanked |= status is RankedStatus.Ranked;
            }
        }

        #endregion

        #region CacheFile

        private static string CacheFileName => Path.Combine(UnityGame.UserDataPath, "BeatLeader", "LeaderboardsCache");

        private static JsonSerializerSettings SerializerSettings => new() {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        static LeaderboardsCache() {
            try {
                if (!File.Exists(CacheFileName)) return;

                var serialized = File.ReadAllText(CacheFileName);
                var fileData = JsonConvert.DeserializeObject<CacheFileData>(serialized, SerializerSettings);
                LastCheckTime = fileData.LastCheckTime;
                foreach (var entry in fileData.Entries) {
                    LeaderboardInfoCache[LeaderboardKey.FromSongDiff(entry.SongInfo, entry.DifficultyInfo)] = entry;
                }
            } catch (Exception e) {
                Plugin.Log.Debug($"LeaderboardsCache load failed! {e}");
            }
        }

        public static void Save() {
            try {
                var fileData = new CacheFileData(LeaderboardInfoCache.Values.ToList(), LastCheckTime);
                var serialized = JsonConvert.SerializeObject(fileData, SerializerSettings);
                FileManager.EnsureDirectoryExists(CacheFileName);
                File.WriteAllText(CacheFileName, serialized);
            } catch (Exception e) {
                Plugin.Log.Debug($"LeaderboardsCache save failed! {e}");
            }
        }

        private class CacheFileData {
            public readonly List<LeaderboardCacheEntry> Entries;
            public readonly long LastCheckTime;

            public CacheFileData(List<LeaderboardCacheEntry> entries, long lastCheckTime) {
                Entries = entries;
                LastCheckTime = lastCheckTime;
            }
        }

        #endregion

        #region Events

        public static event Action CacheWasChangedEvent;

        public static void NotifyCacheWasChanged() {
            RecalculateSortingCache();
            CacheWasChangedEvent?.Invoke();
        }

        #endregion
    }
}