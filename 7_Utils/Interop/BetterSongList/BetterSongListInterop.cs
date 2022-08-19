using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatLeader.DataManager;
using IPA.Loader;
using JetBrains.Annotations;

namespace BeatLeader.Interop.BetterSongList {
    public static class BetterSongListInterop {
        #region TryRegister

        public static void TryRegister() {
            var bslAssembly = PluginManager.GetPluginFromId("BetterSongList")?.Assembly;
            if (bslAssembly == null) return;

            try {
                Register(bslAssembly);
            } catch (Exception e) {
                Plugin.Log.Debug($"BetterSongList interop failed! {e}");
            }
        }

        private static void Register(Assembly bslAssembly) {
            var sorterPrimitiveInterface = bslAssembly.GetType("BetterSongList.SortModels.ISorterPrimitive");
            var sorterWithLegendInterface = bslAssembly.GetType("BetterSongList.SortModels.ISorterWithLegend");
            var transformerPluginInterface = bslAssembly.GetType("BetterSongList.Interfaces.ITransformerPlugin");
            var filterInterface = bslAssembly.GetType("BetterSongList.FilterModels.IFilter");

            var moduleBuilder = ReflectionHell.CreateModuleBuilder("BL_BSL_Interop");

            var highestStarsSorter = Activator.CreateInstance(moduleBuilder.CreateSorterType(
                "HighestStarsSorterType", sorterPrimitiveInterface, sorterWithLegendInterface, transformerPluginInterface,
                "BL Stars", GetHighestStarsMethodInfo, BuildLegendMethodInfo
            ));

            var nominatedFilter = Activator.CreateInstance(moduleBuilder.CreateFilterType(
                "NominatedFilter", filterInterface, transformerPluginInterface,
                "BL Nominated", IsNominatedMethodInfo
            ));

            var qualifiedFilter = Activator.CreateInstance(moduleBuilder.CreateFilterType(
                "QualifiedFilter", filterInterface, transformerPluginInterface,
                "BL Qualified", IsQualifiedMethodInfo
            ));

            var rankedFilter = Activator.CreateInstance(moduleBuilder.CreateFilterType(
                "RankedFilter", filterInterface, transformerPluginInterface,
                "BL Ranked", IsRankedMethodInfo
            ));

            var sortMethods = bslAssembly.GetType("BetterSongList.SortMethods");
            var registerSorterMethodInfo = sortMethods.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic);
            if (registerSorterMethodInfo == null) registerSorterMethodInfo = sortMethods.GetMethod("Register", BindingFlags.Static | BindingFlags.Public)!;
            registerSorterMethodInfo.Invoke(null, new[] { highestStarsSorter });

            var filterMethods = bslAssembly.GetType("BetterSongList.FilterMethods");
            var registerFilterMethodInfo = filterMethods.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic);
            if (registerFilterMethodInfo == null) registerFilterMethodInfo = filterMethods.GetMethod("Register", BindingFlags.Static | BindingFlags.Public)!;
            registerFilterMethodInfo.MakeGenericMethod(nominatedFilter.GetType()).Invoke(null, new[] { nominatedFilter });
            registerFilterMethodInfo.MakeGenericMethod(qualifiedFilter.GetType()).Invoke(null, new[] { qualifiedFilter });
            registerFilterMethodInfo.MakeGenericMethod(rankedFilter.GetType()).Invoke(null, new[] { rankedFilter });
        }

        #endregion

        #region Methods

        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static;
        private static MethodInfo GetHighestStarsMethodInfo => typeof(BetterSongListInterop).GetMethod(nameof(GetHighestStars), Flags);
        private static MethodInfo BuildLegendMethodInfo => typeof(BetterSongListInterop).GetMethod(nameof(BuildLegend), Flags);
        private static MethodInfo IsNominatedMethodInfo => typeof(BetterSongListInterop).GetMethod(nameof(IsNominated), Flags);
        private static MethodInfo IsQualifiedMethodInfo => typeof(BetterSongListInterop).GetMethod(nameof(IsQualified), Flags);
        private static MethodInfo IsRankedMethodInfo => typeof(BetterSongListInterop).GetMethod(nameof(IsRanked), Flags);

        public static bool IsNominated(IPreviewBeatmapLevel level) => level.CachedData()?.IsNominated ?? false;
        public static bool IsQualified(IPreviewBeatmapLevel level) => level.CachedData()?.IsQualified ?? false;
        public static bool IsRanked(IPreviewBeatmapLevel level) => level.CachedData()?.IsRanked ?? false;

        public static IEnumerable<KeyValuePair<string, int>> BuildLegend(IPreviewBeatmapLevel[] levels) {
            return BuildLegendFor(levels, level => {
                var data = level.CachedData();
                return data == null ? "-" : $"{data.HighestStars:F0}";
            });
        }

        public static float? GetHighestStars(IPreviewBeatmapLevel level) {
            return level.CachedData()?.HighestStars;
        }

        #endregion

        #region Utils

        private static IEnumerable<KeyValuePair<string, int>> BuildLegendFor(
            IEnumerable<IPreviewBeatmapLevel> beatmaps,
            Func<IPreviewBeatmapLevel, string> displayValueTransformer,
            int entryLengthLimit = 6,
            int valueLimit = 28
        ) {
            var x1 = beatmaps
                .Select((x, i) => new KeyValuePair<string, int>(displayValueTransformer(x), i))
                .Where(x => x.Key != null)
                .GroupBy(x => x.Key.ToUpperInvariant())
                .ToArray();

            var amt = Math.Min(valueLimit, x1.Length);
            if (amt <= 1) yield break;

            for (var i = 0; i < amt; ++i) {
                var index = (int)Math.Round((double)(x1.Length - 1) / (double)(amt - 1) * (double)i);
                var key = x1[index].Key;
                if (key.Length > entryLengthLimit) {
                    key = key.Substring(0, entryLengthLimit);
                }

                yield return new KeyValuePair<string, int>(key, x1[index].First().Value);
            }
        }

        [CanBeNull]
        private static LeaderboardsCache.SortEntry CachedData(this IPreviewBeatmapLevel level) {
            return !LeaderboardsCache.TryGetSortingInfo(level.Hash(), out var info) ? null : info;
        }

        private static string Hash(this IPreviewBeatmapLevel level) => level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");

        #endregion
    }
}