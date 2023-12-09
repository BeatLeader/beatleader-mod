using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BeatLeader.Replayer.Emulation {
    internal class VRControllersManager : IVRControllersManager {
        [Inject] private readonly OriginalVRControllersProvider _originalVRControllersProvider = null!;
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjectsProvider = null!;
        [Inject] private readonly DiContainer _container = null!;

        private readonly Stack<IVRControllersProvider> _availableProviders = new();
        private readonly HashSet<IVRControllersProvider> _allProviders = new();
        private bool _primaryControllerUnavailable;

        public IVRControllersProvider SpawnControllersProvider(bool primary) {
            if (primary) {
                if (_primaryControllerUnavailable) {
                    throw new InvalidOperationException("Unable to access primary controllers while they are used");
                }
                _primaryControllerUnavailable = true;
                SetProviderActive(_originalVRControllersProvider, true);
                return _originalVRControllersProvider;
            }
            if (_availableProviders.Count is 0) {
                return SpawnProviderInternal();
            }
            var provider = _availableProviders.Pop();
            SetProviderActive(provider, true);
            return provider;
        }

        public void DespawnControllersProvider(IVRControllersProvider provider) {
            if (provider == _originalVRControllersProvider) {
                _primaryControllerUnavailable = false;
                SetProviderActive(provider, false);
                return;
            }
            if (!_allProviders.Contains(provider)) {
                throw new InvalidOperationException("Unable to despawn provider which does not belong to the pool");
            }
            SetProviderActive(provider, false);
            _availableProviders.Push(provider);
        }

        private IVRControllersProvider SpawnProviderInternal() {
            var transform = new GameObject("VRControllersContainer").transform;
            transform.SetParent(_extraObjectsProvider.VRGameCore, false);

            var left = InstantiateVRController(_originalVRControllersProvider.LeftSaber, transform);
            var right = InstantiateVRController(_originalVRControllersProvider.RightSaber, transform);
            var head = InstantiateVRController(_originalVRControllersProvider.Head, transform);
            
            var provider = new GenericVRControllersProvider(left, right, head);
            _allProviders.Add(provider);
            return provider;
        }

        private VRController InstantiateVRController(VRController controller, Transform parent) {
            var go = Object.Instantiate(controller.gameObject, parent, false);
            _container.InjectComponentsInChildren(go);
            return go.GetComponent<VRController>();
        }

        private static void SetProviderActive(IVRControllersProvider provider, bool active) {
            provider.LeftSaber.gameObject.SetActive(active);
            provider.RightSaber.gameObject.SetActive(active);
            provider.Head.gameObject.SetActive(active);
        }
    }
}