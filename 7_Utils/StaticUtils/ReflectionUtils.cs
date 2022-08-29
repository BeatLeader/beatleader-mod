using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace BeatLeader.Utils
{
    public static class ReflectionUtils
    {
        public const BindingFlags DefaultFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static T Cast<T>(this object value, T type)
        {
            return (T)value;
        }
        public static List<FieldInfo> GetFields(this Type targetType, Func<FieldInfo, bool> filter = null, BindingFlags flags = DefaultFlags)
        {
            return targetType.GetFields(flags).Where(x => filter is null ? true : filter(x)).ToList();
        }
        public static List<PropertyInfo> GetProperties(this Type targetType, Func<PropertyInfo, bool> filter = null, BindingFlags flags = DefaultFlags)
        {
            return targetType.GetProperties(flags).Where(x => filter is null ? true : filter(x)).ToList();
        }
        public static List<Type> GetTypes(this Assembly assembly, Func<Type, bool> filter)
        {
            return assembly.GetTypes().Where(filter).ToList();
        }
        public static List<T> ScanAndActivateTypes<T>(this Assembly assembly, Func<T, bool> filter = null, Func<Type, T> activator = null)
        {
            return GetTypes(assembly, x => x == typeof(T) || x.BaseType == typeof(T)).Where(x => !x.IsAbstract)
                .Select(x => activator != null ? activator(x) : (T)Activator.CreateInstance(x))
                .Where(filter != null ? filter : x => true).ToList();
        }
        public static ModuleBuilder CreateModuleBuilder(string name)
        {
            var assemblyName = new AssemblyName(name);
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
            return assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }
        public static PropertyBuilder AddGetOnlyProperty(this TypeBuilder typeBuilder, string name, FieldInfo fieldInfo, MethodInfo overrider = null)
        {
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
        public static void AddDefaultConstructor(this TypeBuilder typeBuilder)
        {
            var ctor0 = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Type.EmptyTypes);
            var ctor0IL = ctor0.GetILGenerator();
            ctor0IL.Emit(OpCodes.Ldarg_0);
            ctor0IL.Emit(OpCodes.Call, typeof(object).GetConstructor(Type.EmptyTypes)!);
            ctor0IL.Emit(OpCodes.Ret);
        }
    }
}
