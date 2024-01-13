using System;
using BeatSaberMarkupLanguage;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BeatLeader.Components {
    [RequireComponent(typeof(Graphic))]
    public class SimpleClickHandler : UIBehaviour, IPointerDownHandler {
        #region FactoryMethods

        public static SimpleClickHandler Custom(GameObject gameObject, Action clickHandler) {
            var component = gameObject.AddComponent<SimpleClickHandler>();
            component.OnClick += clickHandler;
            return component;
        }

        #endregion

        #region Awake

        protected override void Awake() {
            base.Awake();
            GetComponent<Graphic>().raycastTarget = true;
        }

        #endregion

        #region OnClick

        public event Action OnClick;

        public void OnPointerDown(PointerEventData eventData) {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            BeatSaberUI.BasicUIAudioManager.HandleButtonClickEvent();
            OnClick?.Invoke();
        }

        #endregion
    }
}