using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader.Interop.BetterSongList {
    internal static class ReflectionHell {
        #region CreateModuleBuilder

        public static ModuleBuilder CreateModuleBuilder(string name) {
            var assemblyName = new AssemblyName(name);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }

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