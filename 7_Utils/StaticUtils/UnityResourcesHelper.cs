using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatLeader.Utils {
    public static class UnityResourcesHelper {
        public static void LoadResources<T>(this T obj) {
            var flags = BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance;

            var type = obj.GetType();
            var fields = type.GetFields(flags);
            var properties = type.GetProperties(flags);

            foreach (var field in fields) {
                var attribute = field.GetCustomAttribute<FirstResourceAttribute>();
                if (attribute == null || !TryFindObject(field.FieldType,
                    attribute, out var resource)) continue;

                field.SetValue(obj, resource);
            }

            foreach (var property in properties) {
                var attribute = property.GetCustomAttribute<FirstResourceAttribute>();
                if (!property.CanWrite || attribute == null ||
                    !TryFindObject(property.PropertyType,
                    attribute, out var resource)) continue;

                type.GetMethod("set_" + property.Name, flags).Invoke(obj, new object[] { resource });
            }
        }

        private static bool TryFindObject(Type type, FirstResourceAttribute attr, out UnityEngine.Object obj) {
            return (obj = Resources.FindObjectsOfTypeAll(type)
                .FirstOrDefault(x => {
                    if (!(attr.name?.Equals(type) ?? true)) return false;
                    var comp = x as Component;
                    if (comp != null && !(!attr.requireActiveInHierarchy 
                    || comp.gameObject.activeInHierarchy)) return false;
                    return true;
                })) != null;
        }
    }
}