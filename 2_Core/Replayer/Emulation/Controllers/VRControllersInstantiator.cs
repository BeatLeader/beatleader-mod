using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VRControllersInstantiator {
        public VRControllersInstantiator(IVRControllersProvider provider, DiContainer container) {
            _provider = provider;
            _container = container;
        }

        private readonly IVRControllersProvider _provider;
        private readonly DiContainer _container;

        public IVRControllersProvider CreateInstance(string? name = null) {
            var transform = new GameObject(name ?? "InstantiatedVRControllersContainer").transform;

            var leftGo = Object.Instantiate(_provider.LeftSaber.gameObject);
            var rightGo = Object.Instantiate(_provider.RightSaber.gameObject);
            var headGo = Object.Instantiate(_provider.Head.gameObject);

            foreach (var item in new[] { leftGo, rightGo, headGo }) {
                _container.InjectComponentsInChildren(item);
                item.transform.SetParent(transform, false);
            }

            var left = leftGo.GetComponent<VRController>();
            var right = rightGo.GetComponent<VRController>();
            var head = headGo.GetComponent<VRController>();

            return new GenericVRControllersProvider(left, right, head);
        }
    }
}
