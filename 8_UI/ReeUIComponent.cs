using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BeatLeader {
    internal abstract class ReeUIComponent : MonoBehaviour, INotifyPropertyChanged {
        #region Static

        public static T Instantiate<T>(Action<T> postInitAction) where T : ReeUIComponent {
            var result = Instantiate<T>();
            postInitAction.Invoke(result);
            return result;
        }

        public static T Instantiate<T>(bool bindSelf = true) where T : ReeUIComponent {
            var gameObject = new GameObject(typeof(T).Name);
            gameObject.transform.SetParent(GetTemporaryParent());
            var component = gameObject.AddComponent<T>();
            component.BindSelf = bindSelf;
            component.Initialize();
            return component;
        }

        //Protects from Destroy on scene change until proper BSML hierarchy is applied
        private static Transform GetTemporaryParent() {
            var currentScene = SceneManager.GetActiveScene();
            return currentScene.GetRootGameObjects()[0].transform;
        }

        #endregion

        #region Events

        protected virtual void OnInitialize() { }

        protected virtual void OnDispose() { }

        protected virtual void OnBind() { }

        protected virtual void OnActivate(bool firstTime) { }

        protected virtual void OnDeactivate() { }

        #endregion

        #region Initialize

        private const string FallbackContent = "<text text=\"Undefined bsml resource\"/>";

        private bool _initialized;

        private void Initialize() {
            if (_initialized) return;
            Parse();
            _initialized = true;
            OnInitialize();
        }

        private void Parse() {
            _root = transform;
            var type = GetType();
            var customAttribute = type.GetCustomAttribute<ViewDefinitionAttribute>();
            var content = customAttribute == null ? FallbackContent : Utilities.GetResourceContent(type.Assembly, customAttribute.Definition);
            PersistentSingleton<BSMLParser>.instance.Parse(content, gameObject, this);
        }

        #endregion

        #region AlwaysActive

        private bool BindSelf { get; set; }

        #endregion

        #region Root

        private Transform _root;
        private bool _rootWasRequested;

        [UIValue("root"), UsedImplicitly]
        protected virtual Transform Root {
            get {
                _rootWasRequested = true;
                return _root;
            }
        }

        #endregion

        #region OnTransformParentChanged

        private bool _isHierarchySet;

        protected virtual void OnTransformParentChanged() {
            if (_isHierarchySet || !_rootWasRequested) return;
            _isHierarchySet = true;

            AdjustHierarchy();
            OnBind();

            if (!gameObject.activeInHierarchy) return;
            OnActivate(_firstTime);
            _firstTime = false;
        }

        private void AdjustHierarchy() {
            var self = transform;
            if (self.childCount == 0) return;
            var realRoot = self.GetChild(0);
            realRoot.SetParent(self.parent, true);
            _root = realRoot;

            if (!BindSelf) transform.SetParent(GetTemporaryParent());
            gameObject.SetActive(true);
        }

        #endregion

        #region OnEnable & OnDisable

        public bool IsActive { get; private set; }
        private bool _firstTime = true;

        protected virtual void OnEnable() {
            if (!_isHierarchySet) return;
            IsActive = true;
            OnActivate(_firstTime);
            _firstTime = false;
        }

        protected virtual void OnDisable() {
            if (!_isHierarchySet) return;
            OnDeactivate();
            IsActive = false;
        }

        #endregion

        #region OnDestroy

        protected virtual void OnDestroy() {
            OnDispose();
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