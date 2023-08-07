using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.TypeHandlers;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;
using Component = UnityEngine.Component;

namespace BeatLeader.UI.BSML_Addons {
    internal class UIComponentDescriptor<T> : IUIComponentDescriptor<T> {
        static UIComponentDescriptor() {
            var properties = typeof(T)
                .GetMembersWithAttribute<ExternalPropertyAttribute, MemberInfo>();
            var components = typeof(T)
                .GetMembersWithAttribute<ExternalComponentAttribute, MemberInfo>();

            propertySetters = properties.ToDictionary(
                x => x.Value.Name ?? x.Key.Name,
                x => new Action<T, object>(
                    (y, z) => x.Key.SetValueImplicitly(y!, z)));

            var collection = new Dictionary<Type, Func<T, Component>>();
            foreach (var (member, attribute) in components) {
                if (!member.GetMemberTypeImplicitly(out var type)
                    || !type!.IsSubclassOf(typeof(Component))) continue;
                if (collection.ContainsKey(type)) {
                    Plugin.Log.Warn($"Cannot add multiple components of type {type.Name} to {typeof(T).Name}!");
                    continue;
                }
                collection.Add(type, x => {
                    member.GetValueImplicitly(x!, out var val);
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
                constructedObject.transform.SetParent(parent, false);
                component.Content = constructedObject;

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

        protected virtual string Markup { get; } = ReadBsmlOrFallback(typeof(T));

        private static string? _markup;

        private static string ReadBsmlOrFallback(Type componentType) {
            if (_markup != null) return _markup;
            var targetName = $"{componentType.Name}.bsml";

            var resource = componentType.ReadViewDefinition();
            if (resource != string.Empty) return resource;

            var strictMatch = true;
            FindResource: ;
            foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames()) {
                var actualResourceName = GetResourceName(resourceName);
                if (strictMatch ? actualResourceName != targetName : !resourceName.EndsWith(targetName)) continue;
                return _markup = Utilities.GetResourceContent(componentType.Assembly, resourceName);
            }

            if (!strictMatch) return $"<text text=\"Resource not found: {targetName}\" align=\"Center\"/>";
            strictMatch = false;
            goto FindResource;
        }

        private static string GetResourceName(string path) {
            var acc = -1;
            for (var i = path.Length - 1; i >= 0; i--) {
                if (path[i] is not '.') continue;
                if (acc != -1) {
                    acc = i;
                    break;
                }
                acc = i;
            }
            return path.Remove(0, acc + 1);
        }

        #endregion

        #region Parsing

        public GameObject? Content { get; private set; }

        protected virtual GameObject Construct() {
            BSMLParser.instance.Parse(Markup, gameObject, this);
            return transform.GetChild(0).gameObject;
        }

        #endregion

        #region Events

        [UIAction("#post-parse"), UsedImplicitly]
        private void OnAfterParse() { OnInitialize(); }

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