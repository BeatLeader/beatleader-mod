using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using System.Threading;
using BeatLeader.Attributes;
using BeatLeader.DataManager;
using BeatLeader.Utils;
using JetBrains.Annotations;

namespace BeatLeader.Interop {
    [PluginInterop("BetterSongList")]
    public static class BetterSongListInterop {
        #region TryRegister

        [PluginAssembly] private static readonly Assembly _bslAssembly;

        [InteropEntry]
        private static void Init() {
            Register(_bslAssembly);
        }

        private static void Register(Assembly bslAssembly) {
            var sorterPrimitiveInterface = bslAssembly.GetType("BetterSongList.SortModels.ISorterPrimitive");
            var sorterWithLegendInterface = bslAssembly.GetType("BetterSongList.SortModels.ISorterWithLegend");
            var transformerPluginInterface = bslAssembly.GetType("BetterSongList.Interfaces.ITransformerPlugin");
            var filterInterface = bslAssembly.GetType("BetterSongList.FilterModels.IFilter");

            var moduleBuilder = ReflectionUtils.CreateModuleBuilder("BL_BSL_Interop");

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

        #region CreateFilterType

        public static Type CreateFilterType(
            this ModuleBuilder moduleBuilder,
            string typeName,
            Type filterInterface,
            Type transformerPluginInterface,
            string displayedName,
            MethodInfo getValueForMethodInfo
        ) {
            var tb = moduleBuilder.DefineType(
                typeName, TypeAttributes.Public, null,
                new[] { filterInterface, transformerPluginInterface }
            );

            tb.AddDefaultConstructor();
            tb.AddTransformerPluginImplementation(displayedName);
            tb.AddFilterImplementation(getValueForMethodInfo);

            return tb.CreateType();
        }

        #endregion

        #region CreateSorterType

        public static Type CreateSorterType(
            this ModuleBuilder moduleBuilder,
            string typeName,
            Type sorterPrimitiveInterface,
            Type sorterWithLegendInterface,
            Type transformerPluginInterface,
            string displayedName,
            MethodInfo getValueForMethodInfo,
            MethodInfo buildLegendMethodInfo
        ) {
            var tb = moduleBuilder.DefineType(
                typeName, TypeAttributes.Public, null,
                new[] { sorterPrimitiveInterface, sorterWithLegendInterface, transformerPluginInterface }
            );

            tb.AddDefaultConstructor();
            tb.AddTransformerPluginImplementation(displayedName);
            tb.AddSorterImplementation(getValueForMethodInfo, buildLegendMethodInfo);

            return tb.CreateType();
        }

        #endregion

        #region AddTransformerPluginImplementation

        private static void AddTransformerPluginImplementation(
            this TypeBuilder typeBuilder,
            string displayedName
        ) {
            typeBuilder.AddConstantGetOnlyProperty("name", displayedName);
            typeBuilder.AddConstantGetOnlyProperty("visible", true);

            //ContextSwitch()
            var prepareIL = typeBuilder.DefineMethod(
                "ContextSwitch", MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(void), new[] { typeof(SelectLevelCategoryViewController.LevelCategory), typeof(IAnnotatedBeatmapLevelCollection) }
            ).GetILGenerator();
            prepareIL.Emit(OpCodes.Ret);
        }

        #endregion

        #region AddFilterImplementation

        private static void AddFilterImplementation(
            this TypeBuilder typeBuilder,
            MethodInfo getValueForMethodInfo
        ) {
            typeBuilder.AddConstantGetOnlyProperty("isReady", true);

            //GetValueFor()
            var getValueForIL = typeBuilder.DefineMethod(
                "GetValueFor", MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(bool), new[] { typeof(IPreviewBeatmapLevel) }
            ).GetILGenerator();
            getValueForIL.Emit(OpCodes.Ldarg_1);
            getValueForIL.Emit(OpCodes.Call, getValueForMethodInfo);
            getValueForIL.Emit(OpCodes.Ret);

            //Prepare()
            var prepareIL = typeBuilder.DefineMethod(
                "Prepare", MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(Task), new[] { typeof(CancellationToken) }
            ).GetILGenerator();
            prepareIL.Emit(OpCodes.Ldnull);
            prepareIL.Emit(OpCodes.Ret);
        }

        #endregion

        #region AddSorterImplementation

        private static void AddSorterImplementation(
            this TypeBuilder typeBuilder,
            MethodInfo getValueForMethodInfo,
            MethodInfo buildLegendMethodInfo
        ) {
            typeBuilder.AddConstantGetOnlyProperty("isReady", true);

            //GetValueFor()
            var getValueForIL = typeBuilder.DefineMethod(
                "GetValueFor", MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(float?), new[] { typeof(IPreviewBeatmapLevel) }
            ).GetILGenerator();
            getValueForIL.Emit(OpCodes.Ldarg_1);
            getValueForIL.Emit(OpCodes.Call, getValueForMethodInfo);
            getValueForIL.Emit(OpCodes.Ret);

            //BuildLegend()
            var buildLegendIL = typeBuilder.DefineMethod(
                "BuildLegend", MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(IEnumerable<KeyValuePair<string, int>>), new[] { typeof(IPreviewBeatmapLevel[]) }
            ).GetILGenerator();
            buildLegendIL.Emit(OpCodes.Ldarg_1);
            buildLegendIL.Emit(OpCodes.Call, buildLegendMethodInfo);
            buildLegendIL.Emit(OpCodes.Ret);

            //Prepare()
            var prepareIL = typeBuilder.DefineMethod(
                "Prepare", MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(Task), new[] { typeof(CancellationToken) }
            ).GetILGenerator();
            prepareIL.Emit(OpCodes.Ldnull);
            prepareIL.Emit(OpCodes.Ret);
        }

        #endregion

        #region Utils

        private static void AddConstantGetOnlyProperty(this TypeBuilder typeBuilder, string name, bool value) {
            var field = typeBuilder.DefineField($"m_{name}", typeof(bool), FieldAttributes.Private);

            typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, typeof(bool), null);
            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            var getAccessor = typeBuilder.DefineMethod($"get_{name}", getSetAttr, typeof(bool), Type.EmptyTypes);

            var nameGetIL = getAccessor.GetILGenerator();
            nameGetIL.Emit(OpCodes.Ldarg_0);
            nameGetIL.Emit(OpCodes.Ldc_I4, value ? 1 : 0);
            nameGetIL.Emit(OpCodes.Stfld, field);
            nameGetIL.Emit(OpCodes.Ldarg_0);
            nameGetIL.Emit(OpCodes.Ldfld, field);
            nameGetIL.Emit(OpCodes.Ret);
        }

        private static void AddConstantGetOnlyProperty(this TypeBuilder typeBuilder, string name, string value) {
            var field = typeBuilder.DefineField($"m_{name}", typeof(string), FieldAttributes.Private);

            typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, typeof(string), null);
            const MethodAttributes getSetAttr = MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.SpecialName | MethodAttributes.HideBySig;
            var getAccessor = typeBuilder.DefineMethod($"get_{name}", getSetAttr, typeof(string), Type.EmptyTypes);

            var nameGetIL = getAccessor.GetILGenerator();
            nameGetIL.Emit(OpCodes.Ldarg_0);
            nameGetIL.Emit(OpCodes.Ldstr, value);
            nameGetIL.Emit(OpCodes.Stfld, field);
            nameGetIL.Emit(OpCodes.Ldarg_0);
            nameGetIL.Emit(OpCodes.Ldfld, field);
            nameGetIL.Emit(OpCodes.Ret);
        }

        private static void AddDefaultConstructor(this TypeBuilder typeBuilder) {
            var ctor0 = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var ctor0IL = ctor0.GetILGenerator();
            ctor0IL.Emit(OpCodes.Ldarg_0);
            ctor0IL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
            ctor0IL.Emit(OpCodes.Ret);
        }

        #endregion
    }
}