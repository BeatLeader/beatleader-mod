using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;

namespace BeatLeader.Components {
    internal class GenericUIComponentDescriptor<T> : IUIComponentDescriptor<T> where T : class {
        static GenericUIComponentDescriptor() {
            var components = typeof(T)
                .GetMembersWithAttribute<ExternalComponentAttribute, MemberInfo>();

            var setters = new Dictionary<string, Action<T, object>>();
            HandleExternalPropertySetters(GetProperties(typeof(T)), setters);
            propertySetters = setters;

            var collection = new Dictionary<Type, Func<T, Component>>();
            foreach (var (member, attribute) in components) {
                if (!member.GetMemberTypeImplicitly(out var type)
                    || !type!.IsSubclassOf(typeof(Component))) continue;
                if (collection.ContainsKey(type)) {
                    Plugin.Log.Warn($"Cannot add multiple components of type {type.Name} to {typeof(T).Name}!");
                    continue;
                }
                collection.Add(type, x => {
                    member.GetValueImplicitly(x, out var val);
                    return (val as Component)!;
                });
            }
            componentGetters = collection.Select(x => x.Value);
        }

        public virtual string ComponentName { get; } = typeof(T).Name;

        public IDictionary<string, Action<T, object>> ExternalProperties => propertySetters;

        public IEnumerable<Func<T, Component>> ExternalComponents => componentGetters;

        private static readonly IDictionary<string, Action<T, object>> propertySetters;
        private static readonly IEnumerable<Func<T, Component>> componentGetters;

        #region Property Assigning
        
        private static void HandleExternalPropertySetters(
            ICollection<(MemberInfo, string, ExternalPropertyAttribute)> properties,
            IDictionary<string, Action<T, object>> dict,
            string? prefix = null,
            Stack<MemberInfo>? stackMembers = null
        ) {
            prefix = prefix is not null ? $"{prefix}." : null;
            if (properties.Count == 0) return;
            foreach (var (member, propName, attribute) in properties) {
                var name = $"{prefix}{propName}";
                if (dict.ContainsKey(name)) {
                    throw new InvalidOperationException($"Cannot bind multiple properties with the same name \"{member.Name}\"!");
                }
                if (attribute.ExportMode is PropertyExportMode.Inherit) {
                    member.GetMemberTypeImplicitly(out var memberType);
                    var props = GetProperties(memberType!);
                    props = attribute.PropertiesToInherit is { } inheritProps
                        ? props.Where(x => inheritProps.Contains(x.Item2)).ToArray()
                        : props;
                    var valueStack = CloneAndAppendToStack(stackMembers, member);
                    HandleExternalPropertySetters(props, dict, $"{prefix}{attribute.Prefix}", valueStack);
                    continue;
                }
                dict.Add(name, (x, y) => SetProperty(member, AcquireValueByStack(x, stackMembers), y));
            }
        }

        private static Stack<MemberInfo> CloneAndAppendToStack(Stack<MemberInfo>? stack, MemberInfo memberToAppend) {
            var newStack = new Stack<MemberInfo>(stack?.ToArray() ?? Array.Empty<MemberInfo>());
            newStack.Push(memberToAppend);
            return newStack;
        }
        
        private static object? AcquireValueByStack(object obj, IEnumerable<MemberInfo>? members) {
            if (members is null) return obj;
            var value = obj;
            foreach (var member in members) member.GetValueImplicitly(value!, out value);
            return value;
        }
        
        private static void SetProperty(MemberInfo member, object? obj, object value) {
            try {
                if (obj is null) throw new ArgumentNullException(nameof(obj));
                member.GetMemberTypeImplicitly(out var type);
                if (value is string str && type != typeof(string)) {
                    var convertedValue = StringConverter.Convert(str, type!);
                    value = convertedValue ?? throw new InvalidCastException($"Cannot convert {value.GetType().Name} to {type!.Name}");
                }
                member.SetValueImplicitly(obj, value);
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to assign \"{member.Name}\" to object of type {typeof(T).Name}: \n{ex}");
            }
        }

        private static ICollection<(MemberInfo, string, ExternalPropertyAttribute)> GetProperties(Type type) {
            return type.GetMembers(ReflectionUtils.DefaultFlags)
                .Select(static x => {
                    var attr = x.GetCustomAttribute<ExternalPropertyAttribute>();
                    return (x, attr?.Name ?? x.Name, attr);
                })
                .Where(static x => x.attr is not null && x.x is PropertyInfo or FieldInfo)
                .ToArray()!;
        }

        #endregion
    }
}