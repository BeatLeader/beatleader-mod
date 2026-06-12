using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public static LeaderboardCacheEntry PutLeaderboardInfo(
            SongInfo songInfo,
            string leaderboardId,
            DiffInfo diffInfo,
            QualificationInfo qualificationInfo,
            Clan clan,
            bool clanRankingContested
        ) {
            var key = LeaderboardKey.FromSongDiff(songInfo, diffInfo);
            var value = new LeaderboardCacheEntry(leaderboardId, songInfo, diffInfo, qualificationInfo, clan, clanRankingContested);

            LeaderboardInfoCache[key] = value;
            return value;
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

        private static void RecalculateSortingCache(LeaderboardCacheEntry[]? updates = null) {
            if (updates != null) {
                foreach (var entry in updates) {
                    var hash = entry.SongInfo.hash;

                    var sortEntry = SortingCache.ContainsKey(hash) ? SortingCache[hash] : new SortEntry();
                    sortEntry.Update(entry.DifficultyInfo);
                    SortingCache[hash] = sortEntry;
                }
            } else {
                foreach (var pair in LeaderboardInfoCache) {
                    var hash = pair.Key.Hash;

                    var sortEntry = SortingCache.ContainsKey(hash) ? SortingCache[hash] : new SortEntry();
                    sortEntry.Update(pair.Value.DifficultyInfo);
                    SortingCache[hash] = sortEntry;
                }
            }
        }

        public static SortEntry? GetSortingInfo(string hash) {
            if (SortingCache.TryGetValue(hash, out SortEntry result)) {
                return result;
            } else {
                return null;
            }
        }

        public struct SortEntry {
            public float HighestStars;
            public float HighestTechStars;
            public float HighestAccStars;
            public float HighestPassStars;
            public bool IsNominated;
            public bool IsQualified;
            public bool IsRanked;

            public void Update(DiffInfo diffInfo) {
                var status = FormatUtils.GetRankedStatus(diffInfo);

                if (status is RankedStatus.Event or RankedStatus.Nominated or RankedStatus.Qualified or RankedStatus.Ranked) {
                    if (HighestStars < diffInfo.stars) HighestStars = diffInfo.stars;
                    if (HighestTechStars < diffInfo.techRating) HighestTechStars = diffInfo.techRating;
                    if (HighestAccStars < diffInfo.accRating) HighestAccStars = diffInfo.accRating;
                    if (HighestPassStars < diffInfo.passRating) HighestPassStars = diffInfo.passRating;
                }

                IsNominated |= status is RankedStatus.Nominated;
                IsQualified |= status is RankedStatus.Qualified;
                IsRanked |= status is RankedStatus.Ranked;
            }
        }

        #endregion

        #region CacheFile

        private static readonly AppCache<CacheFileData> cache = new("LeaderboardsCache");

        public static void Save() {
            cache.Save();
        }

        public static void Load() {
            cache.Load();
            var fileData = cache.Cache;

            LastCheckTime = fileData.LastCheckTime;

            if (fileData.Entries != null) {
                foreach (var entry in fileData.Entries) {
                    LeaderboardInfoCache[LeaderboardKey.FromSongDiff(entry.SongInfo, entry.DifficultyInfo)] = entry;
                }
            }
        }
        
        private class CacheFileData {
            public List<LeaderboardCacheEntry>? Entries;
            public long LastCheckTime;
        }

        #endregion

        #region Events

        public static event Action CacheWasChangedEvent;

        public static void NotifyCacheWasChanged(LeaderboardCacheEntry[]? leaderboards = null) {
            RecalculateSortingCache(leaderboards);
            CacheWasChangedEvent?.Invoke();
        }

        #endregion
    }
}