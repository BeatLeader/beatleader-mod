using BeatLeader.UI.BSML_Addons;
using UnityEngine;

namespace BeatLeader.Components {
    internal interface IModal {
        bool OffClickCloses { get; }
        
        void Setup(Transform? parent);
        void Pause();
        void Resume();
    }

    [BSMLComponent(Suppress = true)]
    internal abstract class ModalComponentBase<T> : ReeUIComponentV3<T>, IModal where T : ReeUIComponentV3<T> {
        #region Abstraction
        
        protected virtual void OnPause() { }
        protected virtual void OnResume() { }

        #endregion

        #region IModal Impl

        public virtual bool OffClickCloses => true;

        public void Pause() {
            gameObject.SetActive(false);
            Content.SetActive(false);
            OnPause();
        }

        public void Resume() {
            gameObject.SetActive(true);
            Content.SetActive(true);
            OnResume();
        }

        public void Setup(Transform? parent) {
            var active = parent is not null;
            gameObject.SetActive(active);
            Content.SetActive(active);
            ContentTransform.SetParent(parent, false);
        }

        #endregion
    }
}