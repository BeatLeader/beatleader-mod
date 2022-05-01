using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
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
            var targetPostfix = $"{componentType.Name}.bsml";

            foreach (var resourceName in Assembly.GetExecutingAssembly().GetManifestResourceNames()) {
                if (!resourceName.EndsWith(targetPostfix)) continue;
                return Utilities.GetResourceContent(componentType.Assembly, resourceName);
            }

            return $"<text text=\"Resource not found: {targetPostfix}\" align=\"Center\"/>";
        }

        #endregion

        #region Instantiate

        public static T InstantiateOnSceneRoot<T>() where T : ReeUIComponentV2 {
            var lastLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
            var sceneRoot = lastLoadedScene.GetRootGameObjects()[0].transform;
            return Instantiate<T>(sceneRoot);
        }

        public static T Instantiate<T>(Transform parent) where T : ReeUIComponentV2 {
            var component = new GameObject(typeof(T).Name).AddComponent<T>();
            component.Setup(parent);
            return component;
        }

        #endregion

        #region Events

        protected virtual void OnBeforeParse() { }

        protected virtual void OnAfterParse() { }

        protected virtual void OnDispose() { }

        #endregion

        #region UnityEvents

        protected virtual void OnDestroy() {
            if (!IsParsed) return;
            OnDispose();
        }

        #endregion

        #region Setup

        [UIValue("ui-component"), UsedImplicitly]
        private protected virtual Transform Transform { get; set; }

        private Transform _parent;

        private void Setup(Transform parent) {
            _parent = parent;
            Transform = transform;
            Transform.SetParent(parent, false);
            gameObject.SetActive(false);
        }

        #endregion

        #region Parse

        private bool _disablePostParseEvent;
        protected bool IsParsed { get; private set; }

        [UIAction("#post-parse"), UsedImplicitly]
        private protected virtual void PostParse() {
            if (_disablePostParseEvent) return;
            _disablePostParseEvent = true;

            if (IsParsed) OnDispose();
            OnBeforeParse();
            ParseSelf();
            OnAfterParse();

            _disablePostParseEvent = false;
        }

        private void ParseSelf() {
            PersistentSingleton<BSMLParser>.instance.Parse(GetBsmlForType(GetType()), gameObject, this);

            for (var i = 0; i < Transform.childCount; i++) {
                var child = Transform.GetChild(i);
                child.SetParent(Transform.parent, true);
            }

            Transform.SetParent(_parent, false);
            gameObject.SetActive(true);
            IsParsed = true;
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