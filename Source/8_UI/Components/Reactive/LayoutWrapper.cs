using System;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class LayoutWrapper : GameObjectWrapper {
        public LayoutWrapper(Func<GameObject> go) : base(go, false) { }
        
        public Action? OnRecalculateLayout;
        public Action? OnLayoutRecalculated;

        private bool _pendingUpdateCall;

        protected override void OnLateUpdate() {
            if (_pendingUpdateCall) {
                _pendingUpdateCall = false;
                OnLayoutRecalculated?.Invoke();
            }
        }

        protected override void OnRecalculateLayoutSelf() {
            OnRecalculateLayout?.Invoke();
            _pendingUpdateCall = true;
        }
    }
}