using System;
using System.Reflection;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace BeatLeader.Utils {
    internal static class ReflectionUtils {
        #region Constants

        public const BindingFlags DefaultFlags = RequiredFlags | BindingFlags.Instance;
        public const BindingFlags StaticFlags = RequiredFlags | BindingFlags.Static;
        public const BindingFlags RequiredFlags = BindingFlags.Public | BindingFlags.NonPublic;
        public const BindingFlags UniversalFlags = RequiredFlags | BindingFlags.Instance | BindingFlags.Static;

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
            string name,
            FieldInfo fieldInfo,
            MethodInfo? overrider = null) {
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

        public static Dictionary<U, T> GetMembersWithAttribute<T, U>(
            this Type type,
            BindingFlags flags = DefaultFlags) where T : Attribute where U : MemberInfo {
            var dictionary = new Dictionary<U, T>();
            foreach (var member in type.GetMembers(flags)) {
                var attr = member.GetCustomAttribute<T>();
                if (attr == null || member is not U definedMember) continue;
                dictionary.Add(definedMember!, attr);
            }
            return dictionary;
        }

        public static Dictionary<Type, T> GetTypesWithAttribute<T>(this Assembly assembly) where T : Attribute {
            var dictionary = new Dictionary<Type, T>();
            foreach (var type in assembly.GetTypes()) {
                var attr = type.GetCustomAttribute<T>();
                if (attr == null) continue;
                dictionary.Add(type, attr);
            }
            return dictionary;
        }

        #endregion

        #region GetMethod
        
        public static MethodInfo? GetMethod<TDeclarator>(string name) {
            return typeof(TDeclarator).GetMethod(name, UniversalFlags);
        }
        
        public static MethodInfo GetMethod(
            this Type targetType,
            string name,
            BindingFlags bindingAttr = DefaultFlags,
            Type[]? types = null,
            Binder? binder = null) {
            return targetType.GetMethod(name, bindingAttr, binder, types, null);
        }

        #endregion

        #region Casting

        public static bool GetValueImplicitly(this MemberInfo member, object obj, out object? value) {
            value = member switch {
                FieldInfo fld => fld.GetValue(obj),
                PropertyInfo prop => prop.GetValue(obj),
                _ => null
            };
            return value is not null;
        }

        public static bool SetValueImplicitly(this MemberInfo member, object obj, object? value) {
            switch (member) {
                case FieldInfo fld:
                    fld.SetValue(obj, value);
                    break;
                case PropertyInfo prop:
                    prop.SetValue(obj, value);
                    break;
                case EventInfo evt:
                    evt.AddMethod.Invoke(obj, new[] { value! });
                    break;
                default:
                    return false;
            }
            return true;
        }

        public static bool GetMemberTypeImplicitly(this MemberInfo member, out Type? type) {
            return (type = member.MemberType switch {
                MemberTypes.Event => (member as EventInfo)!.EventHandlerType,
                MemberTypes.Field => (member as FieldInfo)!.FieldType,
                MemberTypes.Property => (member as PropertyInfo)!.PropertyType,
                MemberTypes.Method => (member as MethodInfo)!.ReturnType,
                _ => null
            }) != null;
        }

        #endregion
    }
}