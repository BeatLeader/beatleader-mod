using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using BeatLeader.UI.BSML_Addons;
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
    /// <summary>
    /// Base UI component implementation with properties handling
    /// </summary>
    /// <typeparam name="T">Inherited component</typeparam>
    internal abstract class ReeUIComponentV3<T> : ReeUIComponentV3<T, BasicUIComponentDescriptor<T>> where T : ReeUIComponentV3<T> { }

    /// <summary>
    /// Base for UI components
    /// </summary>
    internal abstract class ReeUIComponentV3Base : MonoBehaviour {
        public abstract GameObject Content { get; }
        public abstract RectTransform ContentTransform { get; }
    }

    /// <summary>
    /// Base UI component implementation
    /// </summary>
    /// <typeparam name="T">Inherited component</typeparam>
    /// <typeparam name="TDescriptor">Component descriptor</typeparam>
    [BSMLComponent]
    internal abstract class ReeUIComponentV3<T, TDescriptor> : ReeUIComponentV3Base, INotifyPropertyChanged, IReeUIComponentEventReceiver
        where T : ReeUIComponentV3<T>
        where TDescriptor : IReeUIComponentDescriptor<T>, new() {

        #region Instantiate

        public static T Instantiate(Transform parent) {
            var obj = BSMLTag.CreateObject(parent);
            return (T)obj.GetComponents<ReeUIComponentV3InstanceKeeper>()
                .Select(x => x.Instance)
                .First(x => x is T);
        }

        #endregion

        #region BSML Tag

        protected class Tag : BSMLTag {
            public override string[] Aliases { get; } = { Descriptor.ComponentName };

            public override GameObject CreateObject(Transform parent) {
                var componentGo = new GameObject(Descriptor.ComponentName);
                componentGo.transform.SetParent(parent, false);
                var component = componentGo.AddComponent<T>();

                var constructedObject = component.Construct(parent);
                component._content = constructedObject;
                component._contentTransform = constructedObject.transform as RectTransform;
                //component._contentTransform.SetParent(parent, false);
                component.OnInitialize();

                var externalComponents = constructedObject.AddComponent<ExternalComponents>();
                externalComponents.components.Add(component);
                //since bsml type handlers look onto the ComponentHandler attribute and provide the first
                //component of the specified there type, in case when ui component exported from another
                //ui component we cannot access the main component, so we forced to do such shenanigans
                var instanceKeeper = constructedObject.AddComponent<ReeUIComponentV3InstanceKeeper>();
                instanceKeeper.Instance = component;
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
        protected class Handler : TypeHandler {
            public override Dictionary<string, string[]> Props { get; } =
                Descriptor.ExternalProperties?.ToDictionary(
                    x => x.Key,
                    x => new[] { x.Key })
                ?? new();

            public override void HandleType(BSMLParser.ComponentTypeWithData componentType, BSMLParserParams parserParams) {
                if (componentType.component is not ReeUIComponentV3InstanceKeeper { Instance: T comp }
                    || Descriptor.ExternalProperties is not { } props) return;

                var values = new Dictionary<string, object>();
                foreach (var (key, value) in componentType.valueMap) values.TryAdd(key, value.GetValue());
                foreach (var (key, value) in componentType.data) values.TryAdd(key, value);

                foreach (var (key, value) in values) {
                    if (!props.TryGetValue(key, out var handler)) continue;
                    try {
                        handler(comp, parserParams, value);
                    } catch (Exception ex) {
                        Plugin.Log.Error($"Failed to assign \"{key}\" to {typeof(T).Name}: \n{ex}");
                    }
                }

                var parent = parserParams.host is ReeUIComponentV3Base host ? host.transform : null;
                comp.transform.SetParent(parent, false);
                comp.OnPropertySet();
            }
        }

        #endregion

        #region BSML Data

        protected static readonly TDescriptor Descriptor = new();

        [BSMLTag, UsedImplicitly]
        protected static readonly Tag BSMLTag = new();
        
        [BSMLHandler, UsedImplicitly]
        protected static readonly Handler BSMLHandler = new();
        
        #endregion

        #region Markup

        protected virtual string Markup { get; } = BSMLUtility.ReadMarkupOrFallback(typeof(T));

        protected string CachedMarkup => _markup ??= Markup;

        private static string? _markup;

        #endregion

        #region Parsing

        public bool IsInitialized => Content;

        public sealed override GameObject Content => _content ?? throw new UninitializedComponentException();
        public sealed override RectTransform ContentTransform => _contentTransform ?? throw new UninitializedComponentException();

        private GameObject? _content;
        private RectTransform? _contentTransform;

        protected virtual GameObject Construct(Transform parent) {
            BSMLParser.instance.Parse(CachedMarkup, gameObject, this);
            //we are forced to do such shenanigans since bsml does not return us constructed object
            var components = transform.GetChildren();
            var parsedObject = components.First(Filter).transform;
            parsedObject.SetParent(parent, false);
            return parsedObject.gameObject;

            static bool Filter(Transform x) => x
                .GetComponents<UnityEngine.Component>()
                .All(static x => x is not ReeUIComponentV3Base);
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validates is component initialized or not
        /// </summary>
        /// <exception cref="UninitializedComponentException">Thrown if component is not initialized</exception>
        protected void ValidateAndThrow() {
            if (!IsInitialized || !OnValidation()) throw new UninitializedComponentException();
        }

        /// <summary>
        /// Called with <c>ValidateAndThrow</c>. Used for initialization checks
        /// </summary>
        /// <returns>True if validation has completed, False if not</returns>
        protected virtual bool OnValidation() => true;

        #endregion

        #region Events

        void IReeUIComponentEventReceiver.OnStart() => OnStart();

        void IReeUIComponentEventReceiver.OnStateChange(bool state) => OnStateChange(state);

        void IReeUIComponentEventReceiver.OnRectDimensionsChange() => OnRectDimensionsChange();

        private void Awake() => OnInstantiate();

        private void OnDestroy() {
            OnDispose();
            DestroyImmediate(Content);
        }

        protected virtual void OnPropertySet() { }

        protected virtual void OnInitialize() { }

        protected virtual void OnInstantiate() { }

        protected virtual void OnDispose() { }
        
        protected virtual void OnStart() { }
        
        protected virtual void OnStateChange(bool state) { }
        
        protected virtual void OnRectDimensionsChange() { }

        #endregion

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    internal interface IReeUIComponentEventReceiver {
        void OnStart();
        void OnStateChange(bool state);
        void OnRectDimensionsChange();
    }
    
    internal class ReeUIComponentV3InstanceKeeper : MonoBehaviour {
        public ReeUIComponentV3Base Instance {
            get => _instance;
            set {
                _instance = value;
                _eventReceiver = (IReeUIComponentEventReceiver)value;
            }
        }

        private IReeUIComponentEventReceiver? _eventReceiver;
        private ReeUIComponentV3Base _instance = null!;
        
        private void Start() => _eventReceiver?.OnStart();
        
        private void OnEnable() => _eventReceiver?.OnStateChange(true);
        private void OnDisable() => _eventReceiver?.OnStateChange(false);
        
        private void OnRectTransformDimensionsChange() => _eventReceiver?.OnRectDimensionsChange();
    }
}