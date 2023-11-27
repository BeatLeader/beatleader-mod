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
    /// <summary>
    /// Base UI component implementation with properties handling
    /// </summary>
    /// <typeparam name="T">Inherited component</typeparam>
    internal abstract class ReeUIComponentV3<T> : ReeUIComponentV3<T, GenericUIComponentDescriptor<T>> where T : ReeUIComponentV3<T> { }

    /// <summary>
    /// Base for UI components
    /// </summary>
    internal abstract class ReeUIComponentV3Base : MonoBehaviour {
        public abstract GameObject? Content { get; }
        public abstract Transform? ContentTransform { get; }
    }

    /// <summary>
    /// Base UI component implementation
    /// </summary>
    /// <typeparam name="T">Inherited component</typeparam>
    /// <typeparam name="TDescriptor">Component descriptor</typeparam>
    //TODO: rework so you wont need to specify T generic (quite annoying and non-effective thing)
    internal abstract class ReeUIComponentV3<T, TDescriptor> : ReeUIComponentV3Base, INotifyPropertyChanged, IReeUIComponentV3EventReceiver
        where T : ReeUIComponentV3<T>
        where TDescriptor : IUIComponentDescriptor<T>, new() {

        #region Instantiate

        public static T Instantiate(Transform parent) {
            var obj = bsmlTag.CreateObject(parent);
            return (T)obj.GetComponents<ReeUIComponentV3InstanceKeeper>()
                .Select(x => x.Instance)
                .First(x => x is T);
        }

        #endregion

        #region BSML Tag

        private class Tag : BSMLTag {
            public override string[] Aliases { get; } = { Descriptor.ComponentName };

            public override GameObject CreateObject(Transform parent) {
                var componentGo = new GameObject(Descriptor.ComponentName);
                componentGo.transform.SetParent(parent, false);
                var component = componentGo.AddComponent<T>();

                var constructedObject = component.Construct(parent);
                component._content = constructedObject;
                component._contentTransform = constructedObject.transform;
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
        private class Handler : TypeHandler {
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

        private static readonly Tag bsmlTag = new();
        private static readonly Handler bsmlHandler = new();

        [UsedImplicitly]
        public static (BSMLTag, TypeHandler) GetBSMLData() => (bsmlTag, bsmlHandler);

        #endregion

        #region Markup

        protected virtual string Markup { get; } = BSMLUtility.ReadMarkupOrFallback(typeof(T));

        protected string CachedMarkup => _markup ??= Markup;

        private static string? _markup;

        #endregion

        #region Parsing

        public bool IsInitialized => Content;

        public override GameObject? Content => _content;
        public override Transform? ContentTransform => _contentTransform;

        private GameObject? _content;
        private Transform? _contentTransform;

        protected virtual GameObject Construct(Transform parent) {
            BSMLParser.instance.Parse(CachedMarkup, gameObject, this);
            var parsedObject = transform.GetChild(0);
            parsedObject.SetParent(parent, false);
            return parsedObject.gameObject;
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

        void IReeUIComponentV3EventReceiver.OnStart() => OnStart();

        void IReeUIComponentV3EventReceiver.OnStateChange(bool state) => OnStateChange(state);

        void IReeUIComponentV3EventReceiver.OnRectDimensionsChange() => OnRectDimensionsChange();

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

    internal interface IReeUIComponentV3EventReceiver {
        void OnStart();
        void OnStateChange(bool state);
        void OnRectDimensionsChange();
    }
    
    internal class ReeUIComponentV3InstanceKeeper : MonoBehaviour {
        public ReeUIComponentV3Base Instance {
            get => _instance;
            set {
                _instance = value;
                _eventReceiver = (IReeUIComponentV3EventReceiver)value;
            }
        }

        private IReeUIComponentV3EventReceiver? _eventReceiver;
        private ReeUIComponentV3Base _instance = null!;
        
        private void Start() => _eventReceiver?.OnStart();
        
        private void OnEnable() => _eventReceiver?.OnStateChange(true);
        private void OnDisable() => _eventReceiver?.OnStateChange(false);
        
        private void OnRectTransformDimensionsChange() => _eventReceiver?.OnRectDimensionsChange();
    }
}