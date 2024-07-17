using BeatLeader.Attributes;
using IPA.Loader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Version = Hive.Versioning.Version;

namespace BeatLeader.Utils {
    internal static class InteropLoader {
        #region Init

        public static IReadOnlyCollection<KeyValuePair<Type, PluginInteropAttribute>> Interops { get; private set; } = null!;

        public static void Init() {
            Interops = typeof(InteropLoader).Assembly
                .GetTypesWithAttribute<PluginInteropAttribute>();
            foreach (var interop in Interops) {
                LoadInterop(interop.Key, interop.Value);
            }
        }

        private static void LoadInterop(Type type, PluginInteropAttribute attr) {
            try {
                var plugin = PluginManager.GetPluginFromId(attr.pluginId);
                if (plugin == null) {
                    Plugin.Log.Warn($"Plugin {attr.pluginId} not found, {type.Name} will not be loaded");
                    return;
                }
                if (!IsVersionCorrect(plugin.HVersion, attr.worksUntilVersion!)) {
                    Plugin.Log.Warn($"Plugin {attr.pluginId} version({plugin.HVersion}) not matches specified({attr.worksUntilVersion}), {type.Name} will not be loaded");
                    return;
                }
                var assembly = plugin.Assembly;
                SetPluginAssembly(type, assembly);
                var setAssemblyResult = SetPluginTypes(type, assembly);
                var invokeEntryResult = InvokeEntryMethod(type, !setAssemblyResult);
                SetPluginState(type, invokeEntryResult);
            } catch (Exception ex) {
                Plugin.Log.Warn($"Plugin {attr.pluginId} interop load failed with exception: {ex}");
            }
        }

        private static bool IsVersionCorrect(Version pluginVersion, string specifiedVersion) {
            if (string.IsNullOrEmpty(specifiedVersion)) return true;
            if (!Version.TryParse(specifiedVersion, out var specPlugVer)) return false;
            return pluginVersion < specPlugVer;
        }

        #endregion

        #region Attributes

        private static bool InvokeEntryMethod(Type type, bool smthWasNull) {
            var pair = type.GetMembersWithAttribute<InteropEntryAttribute,
                MethodInfo>(ReflectionUtils.StaticFlags).FirstOrDefault();
            var entry = pair.Key;
            var attr = pair.Value;
            if (entry == null) return true;
            if (smthWasNull && !attr.ignoreOnNull) {
                Plugin.Log.Error($"Ignoring {type.Name} entry because interop was loaded incorrectly!");
                return false;
            }
            try {
                entry.Invoke(null, null);
                return true;
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to execute {type.Name} entry! \r\n{ex}");
            }
            return false;
        }

        private static bool SetPluginAssembly(Type type, Assembly assembly) {
            return SetFirstMemberWithAttribute<PluginAssemblyAttribute, Assembly>(
                type, assembly, ReflectionUtils.StaticFlags);
        }

        private static bool SetPluginState(Type type, bool state) {
            return SetFirstMemberWithAttribute<PluginStateAttribute, bool>(
                type, state, ReflectionUtils.StaticFlags);
        }

        private static bool SetPluginTypes(Type type, Assembly assembly) {
            foreach (var pair in type.GetMembersWithAttribute<PluginTypeAttribute,
                MemberInfo>(ReflectionUtils.StaticFlags)) {
                var member = pair.Key;
                if (!CheckAndLogIsTypeCorrect(assembly, pair.Value.type, out var reqType)
                    || !CheckAndLogIsMemberCorrect(member, typeof(Type), type.Name)) return false;
                member.SetValueImplicitly(null, reqType);
            }
            return true;
        }

        #endregion

        #region Reflection

        private static bool SetFirstMemberWithAttribute<T, U>(Type type, U value, BindingFlags flags) where T : Attribute {
            var members = type.GetMembersWithAttribute<T, MemberInfo>(flags);
            foreach (var pair in members) {
                var member = pair.Key;
                if (member == null || !CheckAndLogIsMemberCorrect
                    (member, typeof(U), type.Name)) continue;
                member.SetValueImplicitly(null, value);
            }
            return false;
        }

        #endregion

        #region Log

        private static bool CheckAndLogIsTypeCorrect(Assembly assembly, string reqType, out Type type) {
            if ((type = assembly.GetType(reqType)) == null) {
                Plugin.Log.Error($"Type {reqType} not found!");
                return false;
            }
            return true;
        }

        private static bool CheckAndLogIsMemberCorrect(MemberInfo member, Type reqType, string interopName) {
            if (!member.GetMemberTypeImplicitly(out var type) || type != reqType) {
                Plugin.Log.Error($"Failed to set {member.Name} in {interopName}, member was of incorrect type!");
                return false;
            }
            if (member is PropertyInfo prop && !prop!.CanWrite) {
                Plugin.Log.Error($"Failed to set {prop.Name} in {interopName}, unable to write property!");
                return false;
            }
            return true;
        }

        #endregion
    }
}
