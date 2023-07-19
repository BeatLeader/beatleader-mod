using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using BeatLeader.Utils;
using BeatSaberMarkupLanguage.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatLeader {
    internal abstract class ReeUIComponentV2 : MonoBehaviour, INotifyPropertyChanged {
        #region BSML Cache

        private static readonly Dictionary<Type, string> BsmlCache = new();

        private static string GetBsmlForType(Type componentType) {
            if (BsmlCache.ContainsKey(componentType)) return BsmlCache[componentType];
            var result = ReadBsmlOrFallback(componentType);
            BsmlCache[componentType] = result;
            return result;
        }

        private static string ReadBsmlOrFallback(Type componentType) {
            var targetName = $"{componentType.Name}.bsml";

            var resource = componentType.ReadViewDefinition();
            if (resource != string.Empty)
                return resource;

            var strictMatch = true;
            FindResource: ;
            foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames()) {
                var actualResourceName = GetResourceName(resourceName);
                if (strictMatch ? actualResourceName != targetName : !resourceName.EndsWith(targetName)) continue;
                return Utilities.GetResourceContent(componentType.Assembly, resourceName);
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

        #region Instantiate

        public static T InstantiateOnSceneRoot<T>(bool parseImmediately = true) where T : ReeUIComponentV2 {
            var lastLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            var sceneRoot = lastLoadedScene.GetRootGameObjects()[0].transform;
            return Instantiate<T>(sceneRoot, parseImmediately);
        }

        public static T Instantiate<T>(Transform parent, bool parseImmediately = true) where T : ReeUIComponentV2 {
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

        protected virtual void OnRootStateChange(bool active) { }

        #endregion

        #region UnityEvents

        protected virtual void OnDestroy() {
            if (!IsHierarchySet) return;
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

        protected bool IsHierarchySet => _state == State.HierarchySet;
        protected bool IsParsed => _state >= State.Parsed;

        private enum State {
            Uninitialized,
            Parsing,
            Parsed,
            HierarchySet
        }

        #endregion

        #region ManualInit

        public void ManualInit(Transform rootNode) {
            DisposeIfNeeded();
            transform.SetParent(rootNode, true);
            ApplyHierarchy();
            OnInitialize();
        }

        #endregion

        #region Content

        private class ContentStateListener : MonoBehaviour {
            public event Action<bool>? StateChangedEvent;

            private void OnEnable() => StateChangedEvent?.Invoke(true);

            private void OnDisable() => StateChangedEvent?.Invoke(false);
        }

        public void SetRootActive(bool active) {
            ValidateAndThrow();
            Content.gameObject.SetActive(active);
        }

        public Transform GetRootTransform() {
            ValidateAndThrow();
            return Content;
        }

        #endregion

        #region Parse

        protected Transform Content { get; private set; }
        protected virtual object ParseHost => this;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void ValidateAndThrow() {
            if (!IsParsed) throw new UninitializedComponentException();
        }

        [UIAction("#post-parse"), UsedImplicitly]
        private protected virtual void PostParse() {
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
            PersistentSingleton<BSMLParser>.instance.Parse(GetBsmlForType(GetType()), gameObject, ParseHost);
            Content = Transform.GetChild(0);
            Content.gameObject.AddComponent<ContentStateListener>().StateChangedEvent += OnRootStateChange;
            _state = State.Parsed;
        }

        private void ApplyHierarchy() {
            ValidateAndThrow();

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