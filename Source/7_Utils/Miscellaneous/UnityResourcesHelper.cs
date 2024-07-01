using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace BeatLeader.Utils {
    internal static class UnityResourcesHelper {
        public static void LoadResources<T>(this T obj) {
            var flags = BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static | BindingFlags.Instance;

            var type = obj!.GetType();
            var members = type.GetMembers(flags);

            foreach (var member in members) {
                var attribute = member.GetCustomAttribute<FirstResourceAttribute>();
                if (attribute == null) continue;

                member.GetMemberTypeImplicitly(out var memberType);
                if (!TryFindObject(memberType!, attribute, out var resource)) continue;

                member.SetValueImplicitly(obj, resource!);
            }
        }

        private static bool TryFindObject(Type type, FirstResourceAttribute attr, out UnityEngine.Object? obj) {
            obj = Resources
                .FindObjectsOfTypeAll(type)
                .FirstOrDefault(Match);
            return obj != null;

            bool Match(UnityEngine.Object obj) {
                var comp = obj as Component;
                var isComp = comp != null;

                var nameIsValid = attr.Name == null || attr.Name == obj.name;
                var parentNameIsValid = attr.ParentName == null || (isComp && comp!.transform.parent && attr.ParentName == comp.transform.parent.name);
                var active = !attr.RequireActiveInHierarchy || (isComp && comp!.gameObject.activeInHierarchy);

                return nameIsValid && parentNameIsValid && active;
            }
        }
    }
}