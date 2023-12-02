using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Parser;
using IPA.Utilities;
using UnityEngine;

namespace BeatLeader.Components {
    internal class BasicUIComponentDescriptor<T> : IReeUIComponentDescriptor<T> where T : class {
        #region UIComponentDescriptor

        static BasicUIComponentDescriptor() {
            var components = typeof(T).GetMembersWithAttribute<ExternalComponentAttribute, MemberInfo>();

            var setters = new Dictionary<string, Action<T, BSMLParserParams, object>>();
            ProcessExternalPropertySetters(GetProperties(typeof(T)), setters);
            propertySetters = setters;

            var collection = new Dictionary<Type, Func<T, Component>>();
            foreach (var pair in components) {
                var member = pair.Key;

                var memberTypeIsCorrect = member.GetMemberTypeImplicitly(out var type);
                var memberSuperclassIsCorrect = type!.IsSubclassOf(typeof(Component));
                if (!memberTypeIsCorrect || !memberSuperclassIsCorrect) continue;

                if (collection.ContainsKey(type)) {
                    Plugin.Log.Warn($"Cannot add multiple components of type {type.Name} to {typeof(T).Name}!");
                    continue;
                }

                var acquireValueDelegate = new Func<T, Component>(
                    x => {
                        member.GetValueImplicitly(x, out var val);
                        return (Component)val!;
                    }
                );
                collection.Add(type, acquireValueDelegate);
            }
            componentGetters = collection.Select(x => x.Value);
        }

        public string ComponentName { get; } = typeof(T).Name;

        public IDictionary<string, Action<T, BSMLParserParams, object>> ExternalProperties => propertySetters;

        public IEnumerable<Func<T, Component>> ExternalComponents => componentGetters;

        private static readonly IDictionary<string, Action<T, BSMLParserParams, object>> propertySetters;
        private static readonly IEnumerable<Func<T, Component>> componentGetters;

        #endregion

        #region Property Assigning

        private static void ProcessExternalPropertySetters(
            ICollection<(MemberInfo, string, ExternalPropertyAttribute)> properties,
            IDictionary<string, Action<T, BSMLParserParams, object>> dict,
            string? prefix = null,
            Stack<MemberInfo>? stackMembers = null
        ) {
            prefix = prefix is not null ? $"{prefix}." : null;
            if (properties.Count == 0) return;

            foreach (var (member, propName, attribute) in properties) {
                var name = $"{prefix}{propName}";
                if (dict.ContainsKey(name)) {
                    throw new InvalidOperationException($"Cannot bind multiple properties with the same name \"{name}\"!");
                }

                if (attribute.ExportMode is PropertyExportMode.Inherit) {
                    member.GetMemberTypeImplicitly(out var memberType);
                    var props = GetProperties(memberType!);

                    if (attribute.PropertiesToInherit is { } inheritProps) {
                        props = props.Where(x => inheritProps.Contains(x.Item2)).ToArray();
                    }

                    var valueStack = CloneAndAppendToStack(stackMembers, member);
                    prefix = $"{prefix}{attribute.Prefix}";
                    ProcessExternalPropertySetters(props, dict, string.IsNullOrEmpty(prefix) ? null : prefix, valueStack);
                    continue;
                }

                dict.Add(
                    name, (x, parserParams, val) => SetProperty(
                        member, AcquireValueByStack(x, stackMembers), parserParams, val
                    )
                );
            }
        }

        private static void SetProperty(MemberInfo member, object? obj, BSMLParserParams parserParams, object value) {
            try {
                if (obj is null) throw new ArgumentNullException(nameof(obj));
                member.GetMemberTypeImplicitly(out var type);
                //is needed to be converted
                if (value is string str && type != typeof(string)) {
                    if (member.MemberType == MemberTypes.Event) {
                        //if target is event
                        if (!parserParams.actions.TryGetValue(str, out var bsmlAction)) {
                            throw new MissingMethodException($"Cannot find a method with name \"{str}\"");
                        }
                        value = bsmlAction.GetField<MethodInfo, BSMLAction>("methodInfo").CreateDelegate(type, parserParams.host);
                    } else if (str.StartsWith("~")) {
                        //if value should be acquired by id
                        if (!parserParams.values.TryGetValue(str, out var bsmlValue)) {
                            throw new MissingMemberException($"Cannot find a value with id \"{str}\"");
                        }
                        value = bsmlValue.GetValue();
                    } else {
                        //if value should be converted directly from string
                        var convertedValue = StringConverter.Convert(str, type!);
                        value = convertedValue ?? throw new InvalidCastException($"Cannot convert {value.GetType().Name} to {type!.Name}");
                    }
                }
                //setting value
                member.SetValueImplicitly(obj, value);
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to assign \"{member.Name}\" to object of type {typeof(T).Name}: \n{ex}");
            }
        }

        private static ICollection<(MemberInfo, string, ExternalPropertyAttribute)> GetProperties(Type type) {
            return type.GetMembers(ReflectionUtils.DefaultFlags)
                .Select(
                    static x => {
                        var attr = x.GetCustomAttribute<ExternalPropertyAttribute>();
                        return (x, attr?.Name ?? x.Name, attr);
                    }
                )
                .Where(static x => x.attr is not null && x.x is PropertyInfo or FieldInfo or EventInfo)
                .ToArray()!;
        }

        #endregion

        #region Stack

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

        #endregion
    }
}