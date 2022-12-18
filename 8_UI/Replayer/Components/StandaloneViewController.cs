using BeatSaberMarkupLanguage.ViewControllers;
using UnityEngine;

namespace BeatLeader.Components {
    internal interface IStandaloneViewController {
        GameObject Container { get; }
        bool IsInitialized { get; }
        bool IsVisible { get; set; }

        void Init();
    }

    internal abstract class StandaloneViewController<T> : BSMLAutomaticViewController, IStandaloneViewController where T : HMUI.Screen {
        public GameObject Container { get; protected set; } = null!;
        public bool IsInitialized { get; private set; }
        public virtual bool IsVisible {
            get => Container?.activeSelf ?? false;
            set => Container?.SetActive(value);
        }
        protected T Screen { get; private set; } = null!;

        public void Init() {
            if (IsInitialized) return;
            Container = new(GetType().Name + "Container");
            Screen = Container.AddComponent<T>();
            OnInit();
            Screen.SetRootViewController(this, AnimationType.None);
            IsInitialized = true;
        }

        protected override sealed void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling) {
            if (firstActivation) OnPreParse();
            base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
        }

        protected virtual void OnInit() {
            Container.transform.SetParent(transform.parent, false);
            transform.SetParent(Container.transform, false);
        }
        protected virtual void OnPreParse() { }
    }
}
