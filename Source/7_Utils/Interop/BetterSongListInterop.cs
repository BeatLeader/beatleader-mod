using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Attributes;
using BeatLeader.DataManager;
using BeatLeader.Utils;

namespace BeatLeader.Interop {
    [PluginInterop("BetterSongList")]
    public static class BetterSongListInterop {
        #region TryRegister

        [PluginAssembly]
        private static readonly Assembly _bslAssembly;

        [InteropEntry]
        private static void Init() {
            var helper = new BslHelper(_bslAssembly);

            helper.AddSort("BL Stars", StarsSortGetter, StarsSortLegend);
            helper.AddSort("BL Tech Rating", TechSortGetter, TechSortLegend);
            helper.AddSort("BL Acc Rating", AccSortGetter, AccSortLegend);
            helper.AddSort("BL Pass Rating", PassSortGetter, PassSortLegend);

            helper.AddFilter("BL Nominated", NominatedFilterGetter);
            helper.AddFilter("BL Qualified", QualifiedFilterGetter);
            helper.AddFilter("BL Ranked", RankedFilterGetter);
        }

        #endregion

        #region Sorts

        public static float? StarsSortGetter(BeatmapLevel level) => level.CachedData()?.HighestStars;
        public static float? TechSortGetter(BeatmapLevel level) => level.CachedData()?.HighestTechStars;
        public static float? AccSortGetter(BeatmapLevel level) => level.CachedData()?.HighestAccStars;
        public static float? PassSortGetter(BeatmapLevel level) => level.CachedData()?.HighestPassStars;

        public static IEnumerable<KeyValuePair<string, int>> StarsSortLegend(BeatmapLevel[] levels) => StarsSortLegend(levels, StarsSortGetter);
        public static IEnumerable<KeyValuePair<string, int>> TechSortLegend(BeatmapLevel[] levels) => StarsSortLegend(levels, TechSortGetter);
        public static IEnumerable<KeyValuePair<string, int>> AccSortLegend(BeatmapLevel[] levels) => StarsSortLegend(levels, AccSortGetter);
        public static IEnumerable<KeyValuePair<string, int>> PassSortLegend(BeatmapLevel[] levels) => StarsSortLegend(levels, PassSortGetter);

        #endregion

        #region Filters

        public static bool NominatedFilterGetter(BeatmapLevel level) => level.CachedData()?.IsNominated ?? false;
        public static bool QualifiedFilterGetter(BeatmapLevel level) => level.CachedData()?.IsQualified ?? false;
        public static bool RankedFilterGetter(BeatmapLevel level) => level.CachedData()?.IsRanked ?? false;

        #endregion

        #region BslHelper

        private class BslHelper {
            private readonly ModuleBuilder _moduleBuilder;
            private readonly Type _sorterPrimitiveInterface;
            private readonly Type _sorterWithLegendInterface;
            private readonly Type _transformerPluginInterface;
            private readonly Type _filterInterface;
            private readonly MethodInfo _registerSorterMethodInfo;
            private readonly MethodInfo _registerFilterMethodInfo;

            public BslHelper(Assembly bslAssembly) {
                _moduleBuilder = ReflectionUtils.CreateModuleBuilder("BL_BSL_Interop");

                _sorterPrimitiveInterface = bslAssembly.GetType("BetterSongList.SortModels.ISorterPrimitive");
                _sorterWithLegendInterface = bslAssembly.GetType("BetterSongList.SortModels.ISorterWithLegend");
                _transformerPluginInterface = bslAssembly.GetType("BetterSongList.Interfaces.ITransformerPlugin");
                _filterInterface = bslAssembly.GetType("BetterSongList.FilterModels.IFilter");

                var sortMethods = bslAssembly.GetType("BetterSongList.SortMethods");
                var tmp = sortMethods.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic);
                if (tmp == null) tmp = sortMethods.GetMethod("Register", BindingFlags.Static | BindingFlags.Public)!;
                _registerSorterMethodInfo = tmp;

                var filterMethods = bslAssembly.GetType("BetterSongList.FilterMethods");
                tmp = filterMethods.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic);
                if (tmp == null) tmp = filterMethods.GetMethod("Register", BindingFlags.Static | BindingFlags.Public)!;
                _registerFilterMethodInfo = tmp;
            }

            public void AddSort(string displayName, Func<BeatmapLevel, float?> getter, Func<BeatmapLevel[], IEnumerable<KeyValuePair<string, int>>> legendBuilder) {
                var type = _moduleBuilder.CreateSorterType(
                    $"{displayName.Replace(' ', '_')}Sort",
                    _sorterPrimitiveInterface, _sorterWithLegendInterface, _transformerPluginInterface,
                    displayName, getter.Method, legendBuilder.Method
                );
                var instance = Activator.CreateInstance(type);
                _registerSorterMethodInfo.Invoke(null, new[] { instance });
            }

            public void AddFilter(string displayName, Func<BeatmapLevel, bool> getter) {
                var type = _moduleBuilder.CreateFilterType(
                    $"{displayName.Replace(' ', '_')}Filter",
                    _filterInterface, _transformerPluginInterface,
                    displayName, getter.Method
                );
                var instance = Activator.CreateInstance(type);
                _registerFilterMethodInfo.MakeGenericMethod(type).Invoke(null, new[] { instance });
            }
        }

        #endregion

        #region Utils

        private static IEnumerable<KeyValuePair<string, int>> StarsSortLegend(
            IEnumerable<BeatmapLevel> levels,
            Func<BeatmapLevel, float?> getter
        ) {
            return BuildLegendFor(levels, level => {
                var stars = getter(level);
                return stars.HasValue ? $"{stars:F0}" : "-";
            });
        }

        private static IEnumerable<KeyValuePair<string, int>> BuildLegendFor(
            IEnumerable<BeatmapLevel> beatmaps,
            Func<BeatmapLevel, string> displayValueTransformer,
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

        private static LeaderboardsCache.SortEntry? CachedData(this BeatmapLevel level) {
            return LeaderboardsCache.GetSortingInfo(level.Hash());
        }

        private static string Hash(this BeatmapLevel level) => level.levelID.Replace(CustomLevelLoader.kCustomLevelPrefixId, "");

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
            //var prepareIL = typeBuilder.DefineMethod(
            //    "ContextSwitch", MethodAttributes.Public | MethodAttributes.Virtual,
            //    typeof(void), new[] { typeof(SelectLevelCategoryViewController.LevelCategory), typeof(IAnnotatedBeatmapLevelCollection) }
            //).GetILGenerator();
            //prepareIL.Emit(OpCodes.Ret);
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
                typeof(bool), new[] { typeof(BeatmapLevel) }
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
                typeof(float?), new[] { typeof(BeatmapLevel) }
            ).GetILGenerator();
            getValueForIL.Emit(OpCodes.Ldarg_1);
            getValueForIL.Emit(OpCodes.Call, getValueForMethodInfo);
            getValueForIL.Emit(OpCodes.Ret);

            //BuildLegend()
            var buildLegendIL = typeBuilder.DefineMethod(
                "BuildLegend", MethodAttributes.Public | MethodAttributes.Virtual,
                typeof(IEnumerable<KeyValuePair<string, int>>), new[] { typeof(BeatmapLevel[]) }
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