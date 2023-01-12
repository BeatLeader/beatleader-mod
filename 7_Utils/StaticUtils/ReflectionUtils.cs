using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace BeatLeader.Utils {
    internal static class ReflectionUtils {
        #region Constants

        public const BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public const BindingFlags StaticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        #endregion

        #region ModuleBuilder

        public static void AddDefaultConstructor(this TypeBuilder typeBuilder) {
            var ctor0 = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var ctor0IL = ctor0.GetILGenerator();
            ctor0IL.Emit(OpCodes.Ldarg_0);
            ctor0IL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
            ctor0IL.Emit(OpCodes.Ret);
        }
        public static ModuleBuilder CreateModuleBuilder(string name) {
            var assemblyName = new AssemblyName(name);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }
        public static PropertyBuilder AddGetOnlyProperty(
            this TypeBuilder typeBuilder,
            string name, FieldInfo fieldInfo,
            MethodInfo overrider = null) {
            var type = fieldInfo.FieldType;
            var getSetAttr = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            var getAccessor = typeBuilder.DefineMethod($"get_{name}", getSetAttr, type, Type.EmptyTypes);
            var nameGetIL = getAccessor.GetILGenerator();

            nameGetIL.Emit(OpCodes.Ldarg_0);
            nameGetIL.Emit(OpCodes.Ldfld, fieldInfo);
            nameGetIL.Emit(OpCodes.Ret);

            if (overrider != null) typeBuilder.DefineMethodOverride(getAccessor, overrider);

            var property = typeBuilder.DefineProperty(name, PropertyAttributes.HasDefault, type, null);
            property.SetGetMethod(getAccessor);
            return property;
        }

        #endregion

        #region Attributes

        public static IReadOnlyCollection<KeyValuePair<PropertyInfo, T>> GetPropertiesWithAttribute<T>(this Type type, BindingFlags flags = DefaultFlags) where T : Attribute {
            Dictionary<PropertyInfo, T> dictionary = new();
            foreach (var property in type.GetProperties(flags)) {
                var attr = property.GetCustomAttribute<T>();
                if (attr == null) continue;
                dictionary.Add(property, attr);
            }
            return dictionary;
        }
        public static IReadOnlyCollection<KeyValuePair<FieldInfo, T>> GetFieldsWithAttribute<T>(this Type type, BindingFlags flags = DefaultFlags) where T : Attribute {
            Dictionary<FieldInfo, T> dictionary = new();
            foreach (var field in type.GetFields(flags)) {
                var attr = field.GetCustomAttribute<T>();
                if (attr == null) continue;
                dictionary.Add(field, attr);
            }
            return dictionary;
        }
        public static IReadOnlyCollection<KeyValuePair<Type, T>> GetTypesWithAttribute<T>(this Assembly assembly) where T : Attribute {
            Dictionary<Type, T> dictionary = new();
            foreach (var type in assembly.GetTypes()) {
                var attr = type.GetCustomAttribute<T>();
                if (attr == null) continue;
                dictionary.Add(type, attr);
            }
            return dictionary;
        }

        #endregion

        #region CustomGetters

        public static MethodInfo GetMethod(
            this Type targetType,
            string name,
            BindingFlags bindingAttr = DefaultFlags,
            Type[] types = null,
            Binder binder = null) {
            return targetType.GetMethod(name, bindingAttr, binder, types, null);
        }

        public static IReadOnlyCollection<Type> GetTypes(
            this Assembly assembly,
            Func<Type, bool> filter) {
            return assembly.GetTypes().Where(filter).ToList();
        }

        public static IReadOnlyCollection<FieldInfo> GetFields(
            this Type targetType,
            Func<FieldInfo, bool> filter = null,
            BindingFlags flags = DefaultFlags) {
            return targetType.GetFields(flags).Where(x => filter?.Invoke(x) ?? true).ToList();
        }

        public static IReadOnlyCollection<PropertyInfo> GetProperties(
            this Type targetType,
            Func<PropertyInfo, bool> filter = null,
            BindingFlags flags = DefaultFlags) {
            return targetType.GetProperties(flags).Where(x => filter?.Invoke(x) ?? true).ToList();
        }


        #endregion

        #region CustomActivator

        public static IReadOnlyCollection<T> ScanAndActivateTypes<T>(
            this Assembly assembly,
            Func<T, bool> filter = null,
            Func<Type, T> activator = null) {
            List<T> types = new();
            foreach (var item in assembly.GetTypes()) {
                if (item != typeof(T) && item.BaseType != typeof(T)) continue;
                var instance = activator != null ? activator(item) : (T)Activator.CreateInstance(item);
                if (!filter?.Invoke(instance) ?? false) continue;
                types.Add(instance);
            }
            return types;
        }

        #endregion

        #region Casting

        public static bool TryDefine<T, U>(this T obj, out U defined) where U : T {
            return (defined = obj is U u ? u : default) != null;
        }

        #endregion
    }
}
