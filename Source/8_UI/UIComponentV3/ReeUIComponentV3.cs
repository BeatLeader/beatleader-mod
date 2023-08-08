using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.TypeHandlers;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Component = UnityEngine.Component;

namespace BeatLeader.Components {
    internal class UIComponentDescriptor<T> : IUIComponentDescriptor<T> where T : class {
        static UIComponentDescriptor() {
            var properties = typeof(T)
                .GetMembersWithAttribute<ExternalPropertyAttribute, MemberInfo>();
            var components = typeof(T)
                .GetMembersWithAttribute<ExternalComponentAttribute, MemberInfo>();

            propertySetters = properties.ToDictionary(
                x => x.Value.Name ?? x.Key.Name,
                x => new Action<T, object>((y, z) => SetProperty(x.Key, y, z)));

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

        #region Property Setter

        private static void SetProperty(MemberInfo member, T obj, object value) {
            try {
                member.GetMemberTypeImplicitly(out var type);
                if (value is string str && type != typeof(string)) {
                    var convertedValue = StringConverter.Convert(str, type!);
                    value = convertedValue ?? throw new InvalidCastException($"Cannot convert {value.GetType().Name} to {type!.Name}");
                }
                member.SetValueImplicitly(obj, value);
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to assign \"{member.Name}\" for {typeof(T).Name}: \n{ex}");
            }
        }

        #endregion
    }

    internal class ReeUIComponentV3<T> : ReeUIComponentV3<T, UIComponentDescriptor<T>> where T : ReeUIComponentV3<T> { }

    internal abstract class ReeUIComponentV3Base : MonoBehaviour { }

    internal abstract class ReeUIComponentV3<T, TDescriptor> : ReeUIComponentV3Base, INotifyPropertyChanged
        where T : ReeUIComponentV3<T>
        where TDescriptor : IUIComponentDescriptor<T>, new() {
        #region BSML Tag

        private class Tag : BSMLTag {
            public override string[] Aliases { get; } = { Descriptor.ComponentName };

            public override GameObject CreateObject(Transform parent) {
                var componentGo = new GameObject(Descriptor.ComponentName);
                componentGo.transform.SetParent(parent, false);
                var component = componentGo.AddComponent<T>();

                var constructedObject = component.Construct();
                component.Content = constructedObject;
                component.ContentTransform = constructedObject.transform;
                component.ContentTransform.SetParent(parent, false);
                component.OnInitialize();

                var externalComponents = constructedObject.AddComponent<ExternalComponents>();
                externalComponents.components.Add(component);
                if (Descriptor.ExternalComponents is { } components) {
                    externalComponents.components.AddRange(components.Select(x => x(component)));
                }

                return constructedObject;
            }
        }

        #endregion

        #region BSML Handler

        [ComponentHandler(typeof(Component))]
        private class Handler : TypeHandler {
            public override Dictionary<string, string[]> Props { get; } =
                Descriptor.ExternalProperties?.ToDictionary(
                    x => x.Key,
                    x => new[] { x.Key })
                ?? new();

            public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams) {
                if (componentType.component is not T comp || Descriptor.ExternalProperties is not { } props) return;

                var values = new Dictionary<string, object>();
                foreach (var (key, value) in componentType.valueMap) values.TryAdd(key, value.GetValue());
                foreach (var (key, value) in componentType.data) values.TryAdd(key, value);

                foreach (var (key, value) in values) {
                    if (!props.TryGetValue(key, out var handler)) continue;
                    try {
                        handler(comp, value);
                    } catch (Exception ex) {
                        Plugin.Log.Error($"Failed to assign \"{key}\" to {typeof(T).Name}: \n{ex}");
                    }
                }
            }
        }

        #endregion

        #region BSML Data

        protected static readonly TDescriptor Descriptor = new();

        private static readonly Tag bsmlTag = new();
        private static readonly Handler bsmlHandler = new();

        [UsedImplicitly]
        public static (BSMLTag, TypeHandler) GetBSMLData() => (bsmlTag, bsmlHandler);

        #endregion

        #region Markup

        protected virtual string Markup { get; } = _markup ??= BSMLUtility.ReadMarkupOrFallback(typeof(T));

        private static string? _markup;

        #endregion

        #region Parsing

        public GameObject? Content { get; private set; }
        public Transform? ContentTransform { get; private set; }

        public bool IsInitialized => Content;

        protected virtual GameObject Construct() {
            BSMLParser.instance.Parse(Markup, gameObject, this);
            return transform.GetChild(0).gameObject;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ValidateAndThrow() {
            if (!IsInitialized) throw new UninitializedComponentException();
        }

        #endregion

        #region Events
        
        private void Awake() { OnInstantiate(); }

        private void OnDestroy() {
            DestroyImmediate(Content);
            OnDispose();
        }

        protected virtual void OnInitialize() { }

        protected virtual void OnInstantiate() { }

        protected virtual void OnDispose() { }

        #endregion

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}