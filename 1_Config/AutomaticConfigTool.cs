using System;
using System.IO;
using System.Collections.Generic;
using BeatLeader.Utils;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BeatLeader {
    internal static class AutomaticConfigTool {
        private const string ConfigsSavePath = @"UserData\BeatLeader\Configs";
        private const string ConfigFileFormat = ".json";

        public static void Load() {
            foreach (var pair in Scan()) {
                TryLoadSettingsForPersistance(pair.Key);
            }
        }
        public static void Save() {
            foreach (var pair in Scan()) {
                TrySaveSettingsForPersistance(pair.Key);
            }
        }
        public static void NotifyTypeChanged(Type type) {
            TrySaveSettingsForPersistance(type);
        }

        private static bool TryLoadSettingsForPersistance(Type type) {
            string pathToFile = GeneratePath(type);
            if (!File.Exists(pathToFile)) return false;
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            try {
                var obj = JObject.Parse(File.ReadAllText(pathToFile));
                foreach (var item in obj) {

                    FieldInfo field = null;
                    if ((field = type.GetField(item.Key, flags)) != null) {
                        field.SetValue(null, item.Value.ToObject(field.FieldType));
                        continue;
                    }
                    PropertyInfo property = null;
                    if ((property = type.GetProperty(item.Key, flags)) != null) {
                        property.SetValue(null, item.Value.ToObject(property.PropertyType));
                        continue;
                    }
                    Plugin.Log.Warn($"Could not find {item.Key}");
                }
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to load config for {type}! \r\n{ex}");
                return false;
            }
            return true;
        }
        private static bool TrySaveSettingsForPersistance(Type type) {
            if (type.GetCustomAttribute<SerializeAutomaticallyAttribute>() == null) return false;

            Dictionary<string, object> convertedFields = new();
            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var pair in type.GetFieldsWithAttribute<SerializeAutomaticallyAttribute>(flags)) {
                var field = pair.Key;
                convertedFields.Add(field.Name, field.GetValue(null));
            }

            foreach (var pair in type.GetPropertiesWithAttribute<SerializeAutomaticallyAttribute>(flags)) {
                var prop = pair.Key;
                if (!prop.CanRead || !prop.CanWrite) continue;
                convertedFields.Add(prop.Name, prop.GetValue(null));
            }

            CreateConfigsFolderIfNeeded();
            File.WriteAllText(GeneratePath(type), JsonConvert.SerializeObject(convertedFields));
            return true;
        }

        private static IReadOnlyCollection<KeyValuePair<Type, SerializeAutomaticallyAttribute>> Scan() {
            return Assembly.GetExecutingAssembly().GetTypesWithAttribute<SerializeAutomaticallyAttribute>();
        }
        private static void CreateConfigsFolderIfNeeded() {
            if (!Directory.Exists(ConfigsSavePath)) Directory.CreateDirectory(ConfigsSavePath);
        }
        private static string GeneratePath(Type type) {
            return GeneratePath(type.GetCustomAttribute
                <SerializeAutomaticallyAttribute>()?.configName ?? type.Name + "Config");
        }
        private static string GeneratePath(string confName) => $"{ConfigsSavePath}\\{confName}{ConfigFileFormat}";
    }
}
