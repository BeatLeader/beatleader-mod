using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader {
    internal abstract class ReeUIComponent : MonoBehaviour, INotifyPropertyChanged {
        #region Static

        public static T Instantiate<T>(Action<T> postInitAction) where T : ReeUIComponent {
            var result = Instantiate<T>();
            postInitAction.Invoke(result);
            return result;
        }

        public static T Instantiate<T>() where T : ReeUIComponent {
            var component = new GameObject(typeof(T).Name).AddComponent<T>();
            component.Initialize();
            return component;
        }

        #endregion

        #region Events

        protected virtual void OnInitialize() { }

        protected virtual void OnDispose() { }

        #endregion

        #region Initialize

        private const string FallbackContent = "<text text=\"Undefined bsml resource\"/>";

        private bool _initialized;

        private void Initialize() {
            if (_initialized) return;
            Parse();
            AdjustHierarchy();
            _initialized = true;
            OnInitialize();
        }

        private void Parse() {
            var type = GetType();
            var customAttribute = type.GetCustomAttribute<ViewDefinitionAttribute>();
            var content = customAttribute == null ? FallbackContent : Utilities.GetResourceContent(type.Assembly, customAttribute.Definition);
            PersistentSingleton<BSMLParser>.instance.Parse(content, gameObject, this);
        }

        private void AdjustHierarchy() {
            _root = transform.GetChild(0);
            _root.SetParent(null);
            transform.SetParent(_root);
        }

        #endregion

        #region OnDestroy

        private void OnDestroy() {
            OnDispose();
        }

        #endregion

        #region Root

        private Transform _root;

        [UIValue("root"), UsedImplicitly]
        protected virtual Transform Root => _root;

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