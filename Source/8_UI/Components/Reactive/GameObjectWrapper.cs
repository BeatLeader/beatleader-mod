using System;
using Reactive;
using UnityEngine;

namespace BeatLeader.UI.Reactive.Components {
    internal class GameObjectWrapper : ReactiveComponent {
        public GameObjectWrapper(Func<GameObject> go, bool rename = true) : base(false) {
            _gameObject = go();
            var name = _gameObject.name;

            ConstructAndInit();
            if (rename) {
                Name = $"{name} (Wrapped)";
            }
        }

        private readonly GameObject _gameObject;

        protected override GameObject Construct() {
            return _gameObject;
        }
    }
}