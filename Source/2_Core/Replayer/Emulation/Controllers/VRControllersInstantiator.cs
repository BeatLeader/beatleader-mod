using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VRControllersInstantiator {
        public VRControllersInstantiator(IVRControllersProvider provider, DiContainer container) {
            _provider = provider;
            _container = container;
            this.LoadResources();
        }

        [FirstResource("VRGameCore", requireActiveInHierarchy: true)]
        private readonly UnityEngine.Transform Origin = null!;

        private readonly IVRControllersProvider _provider;
        private readonly DiContainer _container;
        private bool _originalSabersIsUsed;

        public IVRControllersProvider CreateEmpty() {
            var transform = new GameObject("EmptyControllers").transform;
            var left = new GameObject("left").AddComponent<VRControllerEmulator>();
            var right = new GameObject("right").AddComponent<VRControllerEmulator>();
            var head = new GameObject("head").AddComponent<VRControllerEmulator>();
            foreach (var item in new[] { left, right, head }) {
                item.transform.SetParent(transform, false);
            }
            return new GenericVRControllersProvider(left, right, head);
        }

        public IVRControllersProvider CreateInstance(string? name = null) {
            if (!_originalSabersIsUsed) {
                _originalSabersIsUsed = true;
                return _provider;
            }

            var transform = new GameObject(name ?? "InstantiatedVRControllersContainer").transform;

            transform.SetParent(Origin, false);

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
