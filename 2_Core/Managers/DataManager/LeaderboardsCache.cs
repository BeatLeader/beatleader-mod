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

        public static void PutLeaderboardInfo(SongInfo songInfo, string leaderboardId, DiffInfo diffInfo, QualificationInfo qualificationInfo, ClanRankingInfo clanRanking, bool clanRankingContested) {
            var key = LeaderboardKey.FromSongDiff(songInfo, diffInfo);
            LeaderboardInfoCache[key] = new LeaderboardCacheEntry(leaderboardId, songInfo, diffInfo, qualificationInfo, clanRanking, clanRankingContested);
        }

        public readonly struct LeaderboardCacheEntry {
            public readonly string LeaderboardId;
            public readonly SongInfo SongInfo;
            public readonly DiffInfo DifficultyInfo;
            public readonly QualificationInfo QualificationInfo;
            public readonly ClanRankingInfo ClanRankingInfo;
            public readonly bool ClanRankingContested;

            public LeaderboardCacheEntry(string leaderboardId, SongInfo songInfo, DiffInfo difficultyInfo, QualificationInfo qualificationInfo, ClanRankingInfo clanRanking, bool clanRankingContested) {
                SongInfo = songInfo;
                DifficultyInfo = difficultyInfo;
                QualificationInfo = qualificationInfo;
                LeaderboardId = leaderboardId;
                ClanRankingInfo = clanRanking;
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
                var stars = pair.Value.DifficultyInfo.stars;
                var status = FormatUtils.GetRankedStatus(pair.Value.DifficultyInfo);
                var isNominated = status is RankedStatus.Nominated;
                var isQualified = status is RankedStatus.Qualified;
                var isRanked = status is RankedStatus.Ranked;

                if (SortingCache.ContainsKey(hash)) {
                    var entry = SortingCache[hash];
                    if (entry.HighestStars < stars) entry.HighestStars = stars;
                    entry.IsNominated |= isNominated;
                    entry.IsQualified |= isQualified;
                    entry.IsRanked |= isRanked;
                } else {
                    SortingCache[hash] = new SortEntry(stars, isNominated, isQualified, isRanked);
                }
            }
        }

        public static bool TryGetSortingInfo(string hash, out SortEntry sortingInfo) {
            if (SortingCache.ContainsKey(hash)) {
                sortingInfo = SortingCache[hash];
                return true;
            }

            sortingInfo = null;
            return false;
        }

        public class SortEntry {
            public float HighestStars;
            public bool IsNominated;
            public bool IsQualified;
            public bool IsRanked;

            public SortEntry(float highestStars, bool isNominated, bool isQualified, bool isRanked) {
                HighestStars = highestStars;
                IsNominated = isNominated;
                IsQualified = isQualified;
                IsRanked = isRanked;
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