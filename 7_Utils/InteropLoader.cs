using BeatLeader.Attributes;
using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BeatLeader.Utils {
    internal static class InteropLoader {
        #region Init

        public static IReadOnlyCollection<KeyValuePair<Type, PluginInteropAttribute>> Interops { get; private set; }

        public static void Init() {
            Interops = typeof(InteropLoader).Assembly.GetTypesWithAttribute<PluginInteropAttribute>();
            foreach (var interop in Interops) {
                LoadInterop(interop.Key, interop.Value);
            }
        }
        private static void LoadInterop(Type type, PluginInteropAttribute attr) {
            var assembly = PluginManager.GetPluginFromId(attr.pluginId)?.Assembly;
            if (assembly == null) {
                Plugin.Log.Warn($"Plugin {attr.pluginId} not found, {type.Name} will not be loaded");
                return;
            }
            SetPluginAssembly(type, assembly);
            bool setAssemblyResult = SetPluginTypes(type, assembly);
            bool invokeEntryResult = InvokeEntryMethod(type, setAssemblyResult);
            SetPluginState(type, invokeEntryResult);
        }

        #endregion

        #region Attributes

        private static bool InvokeEntryMethod(Type type, bool smthWasNull) {
            InteropEntryAttribute attr = null;
            var entry = type.GetMethods(ReflectionUtils.StaticFlags)
                .FirstOrDefault(x => (attr = x.GetCustomAttribute<InteropEntryAttribute>()) != null);
            if (entry != null) {
                if (smthWasNull && !attr.ignoreOnNull) {
                    Plugin.Log.Warn($"Ignoring {type.Name} entry because interop was loaded incorrectly!");
                    return false;
                }
                try {
                    entry.Invoke(null, null);
                    return true;
                } catch (Exception ex) {
                    Plugin.Log.Error($"Failed to execute {type.Name} entry! \r\n{ex}");
                }
            }
            return false;
        }
        private static bool SetPluginAssembly(Type type, Assembly assembly) {
            return SetFirstPropertyOrFieldWithAttribute<PluginAssemblyAttribute, Assembly>(
                type, assembly, ReflectionUtils.StaticFlags);
        }
        private static bool SetPluginState(Type type, bool state) {
            return SetFirstPropertyOrFieldWithAttribute<PluginStateAttribute, bool>(
                type, state, ReflectionUtils.StaticFlags);
        }
        private static bool SetPluginTypes(Type type, Assembly assembly) {
            bool smthWasNull = false;
            foreach (var pair in type.GetFieldsWithAttribute<PluginTypeAttribute>(ReflectionUtils.StaticFlags)) {
                var field = pair.Key;
                if (!CheckAndLogIsTypeCorrect(assembly, pair.Value.type, out var reqType)) {
                    smthWasNull = true;
                    continue;
                }
                if (CheckAndLogIsFieldCorrect(field, typeof(Type), type.Name)) {
                    field.SetValue(null, reqType);
                } else {
                    smthWasNull = true;
                }
            }
            foreach (var pair in type.GetPropertiesWithAttribute<PluginTypeAttribute>(ReflectionUtils.StaticFlags)) {
                var property = pair.Key;
                if (!CheckAndLogIsTypeCorrect(assembly, pair.Value.type, out var reqType)) {
                    smthWasNull = true;
                    continue;
                }
                if (CheckAndLogIsPropertyCorrect(property, typeof(Type), type.Name)) {
                    property.SetValue(null, assembly.GetType(pair.Value.type));
                } else {
                    smthWasNull = true;
                }
            }
            return smthWasNull;
        }

        #endregion

        #region Reflection

        private static bool SetFirstPropertyOrFieldWithAttribute<T, U>(Type type, U value, BindingFlags flags) where T : Attribute {
            var field = type.GetFields(flags)
                .FirstOrDefault(x => x.GetCustomAttribute<T>() != null);
            if (field != null && CheckAndLogIsFieldCorrect(field, typeof(U), type.Name)) {
                field.SetValue(null, value);
                return true;
            }

            var property = type.GetProperties(flags)
                .FirstOrDefault(x => x.GetCustomAttribute<T>() != null);
            if (property != null && CheckAndLogIsPropertyCorrect(property, typeof(U), type.Name)) {
                property.SetValue(null, value);
                return true;
            }

            return false;
        }

        #endregion

        #region Log

        private static bool CheckAndLogIsTypeCorrect(Assembly assembly, string reqType, out Type type) {
            type = assembly.GetType(reqType);
            if (type == null) {
                Plugin.Log.Error($"Type {reqType} not found!");
                return false;
            }
            return true;
        }
        private static bool CheckAndLogIsPropertyCorrect(PropertyInfo prop, Type reqType, string interopName) {
            if (prop.CanWrite) {
                if (prop.PropertyType == reqType) {
                    return true;
                } else {
                    Plugin.Log.Error($"Failed to set {prop.Name} in {interopName}, property was of incorrect type!");
                }
            } else {
                Plugin.Log.Error($"Failed to set {prop.Name} in {interopName}, unable to write property!");
            }
            return false;
        }
        private static bool CheckAndLogIsFieldCorrect(FieldInfo field, Type reqType, string interopName) {

            if (field.FieldType == reqType) {
                return true;
            } else {
                Plugin.Log.Error($"Failed to set {field.Name} in {interopName}, field was of incorrect type!");
            }
            return false;
        }

        #endregion
    }
}
