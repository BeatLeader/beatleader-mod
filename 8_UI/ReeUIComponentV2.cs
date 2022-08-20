using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using BeatLeader.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace BeatLeader
{
    internal abstract class ReeUIComponentV2 : MonoBehaviour, INotifyPropertyChanged
    {
        #region BSML Cache

        private static readonly Dictionary<Type, string> BsmlCache = new();

        private static string GetBsmlForType(Type componentType) {
            if (BsmlCache.ContainsKey(componentType)) return BsmlCache[componentType];
            var result = ReadBsmlOrFallback(componentType);
            BsmlCache[componentType] = result;
            return result;
        }

        private static string ReadBsmlOrFallback(Type componentType) {
            var targetPostfix = $"{componentType.Name}.bsml";

            string resource = componentType.ReadViewDefinition();
            if (resource != string.Empty)
                return resource;

            foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames()) {
                if (!resourceName.EndsWith(targetPostfix)) continue;
                return Utilities.GetResourceContent(componentType.Assembly, resourceName);
            }

            return $"<text text=\"Resource not found: {targetPostfix}\" align=\"Center\"/>";
        }

        #endregion

        #region Instantiate

        public static T InstantiateOnSceneRoot<T>(bool parseImmediately = true) where T : ReeUIComponentV2 {
            var lastLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            var sceneRoot = lastLoadedScene.GetRootGameObjects()[0].transform;
            return Instantiate<T>(sceneRoot, parseImmediately);
        }

        public static T Instantiate<T>(Transform parent, bool parseImmediately = true) where T : ReeUIComponentV2
        {
            var component = new GameObject(typeof(T).Name).AddComponent<T>();
            component.OnInstantiate();
            component.Setup(parent, parseImmediately);
            return component;
        }

        #endregion

        #region Events

        protected virtual void OnInstantiate() { }

        protected virtual void OnInitialize() { }

        protected virtual void OnDispose() { }

        #endregion

        #region UnityEvents

        protected virtual void OnDestroy()
        {
            if (!IsParsed) return;
            OnDispose();
        }

        #endregion

        #region Setup

        [UIValue("ui-component"), UsedImplicitly]
        private protected virtual Transform Transform { get; set; }

        private Transform _parent;

        protected void Setup(Transform parent, bool parseImmediately) {
            _parent = parent;
            Transform = transform;
            Transform.SetParent(parent, false);
            if (parseImmediately) ParseSelfIfNeeded();
            gameObject.SetActive(false);
        }

        public void SetParent(Transform parent) {
            _parent = parent;
            Transform.SetParent(parent, false);
        }

        #endregion

        #region State

        private State _state = State.Uninitialized;

        protected bool IsParsed => _state == State.HierarchySet;

        private enum State {
            Uninitialized,
            Parsing,
            Parsed,
            HierarchySet
        }

        #endregion

        #region Parse
        protected Transform Content { get; private set; }

        [UIAction("#post-parse"), UsedImplicitly]
        private protected virtual void PostParse()
        {
            if (_state == State.Parsing) return;
            DisposeIfNeeded();
            ParseSelfIfNeeded();
            ApplyHierarchy();
            OnInitialize();
        }

        private void DisposeIfNeeded() {
            if (_state != State.HierarchySet) return;
            OnDispose();
            _state = State.Uninitialized;
        }

        private void ParseSelfIfNeeded() {
            if (_state != State.Uninitialized) return;
            _state = State.Parsing;
            PersistentSingleton<BSMLParser>.instance.Parse(GetBsmlForType(GetType()), gameObject, this);
            Content = Transform.GetChild(0);
            _state = State.Parsed;
        }

        private void ApplyHierarchy() {
            if (_state != State.Parsed) throw new Exception("Component isn't parsed!");
            
            Content.SetParent(Transform.parent, true);

            Transform.SetParent(_parent, false);
            gameObject.SetActive(true);
            _state = State.HierarchySet;
        }

        #endregion

        #region NotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}