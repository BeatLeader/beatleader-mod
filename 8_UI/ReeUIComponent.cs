using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

namespace BeatLeader {
    internal abstract class ReeUIComponent : MonoBehaviour, INotifyPropertyChanged {
        #region Static

        public static T Instantiate<T>(Action<T> postInitAction) where T : ReeUIComponent {
            var result = Instantiate<T>();
            postInitAction.Invoke(result);
            return result;
        }

        public static T Instantiate<T>() where T : ReeUIComponent {
            var component = CreateRootGameObject(typeof(T).Name).AddComponent<T>();
            component.Initialize();
            return component;
        }

        private static GameObject CreateRootGameObject(string name) {
            var gameObject = new GameObject(name);
            gameObject.AddComponent<VerticalLayoutGroup>();
            gameObject.AddComponent<LayoutElement>();
            return gameObject;
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
            var type = GetType();
            var customAttribute = type.GetCustomAttribute<ViewDefinitionAttribute>();
            var content = customAttribute == null ? FallbackContent : Utilities.GetResourceContent(type.Assembly, customAttribute.Definition);
            PersistentSingleton<BSMLParser>.instance.Parse(content, gameObject, this);
            _initialized = true;
            OnInitialize();
        }

        #endregion

        #region OnDestroy

        private void OnDestroy() {
            OnDispose();
        }

        #endregion

        #region Root

        [UIValue("root"), UsedImplicitly]
        protected virtual Transform Root => transform;

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