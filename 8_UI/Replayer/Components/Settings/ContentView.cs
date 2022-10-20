using System;
using UnityEngine;

namespace BeatLeader.Components {
    internal class ContentView : ReeUIComponentV2 {
        public event Action ContentWasPresentedEvent;
        public event Action ContentWasDismissedEvent;

        private Transform _viewOriginalParent;
        private ContentView _presentedView;

        public void PresentView(ContentView view) {
            _presentedView = view;
            _viewOriginalParent = view.Content.parent;
            _presentedView.SetParent(Content.parent);
            gameObject.SetActive(false);
            ContentWasPresentedEvent?.Invoke();
        }
        public bool DismissView() {
            if (_presentedView == null) return false;
            _presentedView.SetParent(_viewOriginalParent);
            gameObject.SetActive(true);
            ContentWasDismissedEvent?.Invoke();
            _presentedView = null;
            _viewOriginalParent = null;
            return true;
        }
    }
}
