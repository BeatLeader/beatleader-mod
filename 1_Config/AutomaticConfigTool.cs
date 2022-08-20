using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BeatLeader.Utils;
using Newtonsoft.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;

namespace BeatLeader
{
    public class AutomaticConfigTool
    {
        private const string ConfigsSavePath = @"UserData\BeatLeader\Configs\";
        private const string ConfigFileFormat = ".json";

        public static void Load() => Scan().ForEach(x => TryLoadSettingsForPersistance(x));
        public static void Save() => Scan().ForEach(x => TrySaveSettingsForPersistance(x));
        public static void NotifyTypeChanged(Type type) => TrySaveSettingsForPersistance(type);

        private static List<Type> Scan()
        {
            return Assembly.GetExecutingAssembly().GetTypes(x => x.GetCustomAttributes()
            .FirstOrDefault(x => x.GetType() == typeof(SerializeAutomaticallyAttribute)) != null);
        }
        private static bool TryLoadSettingsForPersistance(Type type)
        {
            string pathToFile = GeneratePath(type);
            if (!File.Exists(pathToFile)) return false;

            try
            {
                var obj = JObject.Parse(File.ReadAllText(pathToFile));
                foreach (var item in obj)
                {
                    FieldInfo field = null;
                    if ((field = type.GetField(item.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) != null)
                    {
                        field.SetValue(null, item.Value.ToObject(field.FieldType));
                        continue;
                    }
                    PropertyInfo property = null;
                    if ((property = type.GetProperty(item.Key, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)) != null)
                    {
                        property.SetValue(null, item.Value.ToObject(property.PropertyType));
                        continue;
                    }
                    Plugin.Log.Warn($"Could not find {item.Key}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Critical($"An unhandled exception occured during attempting to load config for {type}! {ex}");
                return false;
            }
            return true;
        }
        private static bool TrySaveSettingsForPersistance(Type type)
        {
            Dictionary<string, object> convertedFields = new();
            type.GetFields(x => x.GetCustomAttribute(typeof(SerializeAutomaticallyAttribute)) != null,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .ForEach(x => convertedFields.Add(x.Name, x.GetValue(null)));
            type.GetProperties(x => x.GetCustomAttribute(typeof(SerializeAutomaticallyAttribute)) != null
                 && x.CanRead && x.CanWrite, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                .ForEach(x => convertedFields.Add(x.Name, x.GetValue(null)));

            File.WriteAllText(GeneratePath(type), JsonConvert.SerializeObject(convertedFields));
            return true;
        }
        private static string GeneratePath(Type type) => $"{ConfigsSavePath}{type.Name}Config{ConfigFileFormat}";
    }
}
