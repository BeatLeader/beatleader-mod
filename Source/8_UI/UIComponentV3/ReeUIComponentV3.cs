using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

namespace BeatLeader.Components {
    internal class ReeUIComponentV3<T> : ReeUIComponentV3<T, GenericUIComponentDescriptor<T>> where T : ReeUIComponentV3<T> { }

    internal abstract class ReeUIComponentV3Base : MonoBehaviour {
        public abstract GameObject? Content { get; }
        public abstract Transform? ContentTransform { get; }
    }

    internal class ReeUIComponentV3InstanceKeeper : MonoBehaviour {
        public ReeUIComponentV3Base instance = null!;
    }

    internal abstract class ReeUIComponentV3<T, TDescriptor> : ReeUIComponentV3Base, INotifyPropertyChanged
        where T : ReeUIComponentV3<T>
        where TDescriptor : IUIComponentDescriptor<T>, new() {
        #region Instantiate

        public static T Instantiate(Transform parent) {
            var obj = bsmlTag.CreateObject(parent);
            return (T)obj.GetComponent<ExternalComponents>().components.First(x => x is T);
        }

        #endregion

        #region BSML Tag

        private class Tag : BSMLTag {
            public override string[] Aliases { get; } = { Descriptor.ComponentName };

            public override GameObject CreateObject(Transform parent) {
                var componentGo = new GameObject(Descriptor.ComponentName);
                componentGo.transform.SetParent(parent, false);
                var component = componentGo.AddComponent<T>();

                var constructedObject = component.Construct();
                component._content = constructedObject;
                component._contentTransform = constructedObject.transform;
                component._contentTransform.SetParent(parent, false);
                component.OnInitialize();

                var externalComponents = constructedObject.AddComponent<ExternalComponents>();
                externalComponents.components.Add(component);
                //since bsml type handlers look onto the ComponentHandler attribute and provide the first
                //component of the specified there type, in case when ui component exported from another
                //ui component we cannot access the main component, so we forced to do such shenanigans
                var instanceKeeper = constructedObject.AddComponent<ReeUIComponentV3InstanceKeeper>();
                instanceKeeper.instance = component;
                externalComponents.components.Add(instanceKeeper);
                if (Descriptor.ExternalComponents is { } components) {
                    externalComponents.components.AddRange(components.Select(x => x(component)));
                }
                return constructedObject;
            }
        }

        #endregion

        #region BSML Handler

        [ComponentHandler(typeof(ReeUIComponentV3InstanceKeeper))]
        private class Handler : TypeHandler {
            public override Dictionary<string, string[]> Props { get; } =
                Descriptor.ExternalProperties?.ToDictionary(
                    x => x.Key,
                    x => new[] { x.Key })
                ?? new();

            public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams) {
                if (componentType.component is not ReeUIComponentV3InstanceKeeper { instance: T comp }
                    || Descriptor.ExternalProperties is not { } props) return;

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

        public bool IsInitialized => Content;

        public override GameObject? Content => _content;
        public override Transform? ContentTransform => _contentTransform;

        private GameObject? _content;
        private Transform? _contentTransform;

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