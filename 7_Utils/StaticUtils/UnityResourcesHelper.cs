using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatLeader.Utils
{
    public static class UnityResourcesHelper
    {
        public static void LoadResources<T>(this T obj)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance;

            var type = obj.GetType();
            var fields = type.GetFields(flags);
            var properties = type.GetProperties(flags);

            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<FirstResourceAttribute>();
                if (attribute == null) continue;

                var resource = Resources.FindObjectsOfTypeAll(field.FieldType)
                    .FirstOrDefault(x => attribute.name == null || x.name == attribute.name);
                if (resource == null) continue;

                field.SetValue(obj, resource);
            }

            foreach (var property in properties)
            {
                var attribute = property.GetCustomAttribute<FirstResourceAttribute>();
                if (!property.CanWrite || attribute == null) continue;

                var resource = Resources.FindObjectsOfTypeAll(property.PropertyType)
                    .FirstOrDefault(x => attribute.name == null || x.name == attribute.name);
                if (resource == null) continue;

                type.GetMethod("set_" + property.Name, flags).Invoke(obj, new object[] { resource });
            }
        }
    }
}