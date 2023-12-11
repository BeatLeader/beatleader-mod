using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BeatLeader.Replayer.Emulation {
    internal class VRControllersManager : IVRControllersManager {
        #region CustomVRController

        private class CustomVRController : VRController {
            public Material? bladeMaterial;
            public Material? handleMaterial;

            private void OnDestroy() {
                Destroy(bladeMaterial);
                Destroy(handleMaterial);
            }
        }

        #endregion

        #region Injection

        [Inject] private readonly OriginalVRControllersProvider _originalVRControllersProvider = null!;
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjectsProvider = null!;

        #endregion

        #region Spawn & Despawn

        private readonly Stack<IVRControllersProvider> _availableProviders = new();
        private readonly HashSet<IVRControllersProvider> _allProviders = new();
        private bool _primaryControllerUnavailable;

        public IVRControllersProvider SpawnControllers(IVirtualPlayerBase player, bool primary) {
            if (primary) {
                if (_primaryControllerUnavailable) {
                    throw new InvalidOperationException("Unable to access primary controllers while they are used");
                }
                _primaryControllerUnavailable = true;
                SetControllersActive(_originalVRControllersProvider, true);
                return _originalVRControllersProvider;
            }
            var provider = _availableProviders.Count switch {
                0 => SpawnProviderInternal(),
                _ => _availableProviders.Pop()
            };
            var color = player.Replay.ReplayData.Player?.AccentColor ?? Color.white;
            RefreshSabers(provider, color);
            SetControllersActive(provider, true);
            return provider;
        }

        private IVRControllersProvider SpawnProviderInternal() {
            var transform = new GameObject("VRControllersContainer").transform;
            transform.SetParent(_extraObjectsProvider.VRGameCore, false);

            var left = InstantiateSaber(transform);
            var right = InstantiateSaber(transform);
            var head = Object.Instantiate(_originalVRControllersProvider.Head, transform);

            var provider = new GenericVRControllersProvider(left, right, head);
            _allProviders.Add(provider);
            return provider;
        }

        public void DespawnControllers(IVRControllersProvider provider) {
            if (provider == _originalVRControllersProvider) {
                _primaryControllerUnavailable = false;
                SetControllersActive(provider, false);
                return;
            }
            if (!_allProviders.Contains(provider)) {
                throw new InvalidOperationException("Unable to despawn provider which does not belong to the pool");
            }
            SetControllersActive(provider, false);
            _availableProviders.Push(provider);
        }

        private static void SetControllersActive(IVRControllersProvider provider, bool active) {
            provider.LeftSaber.gameObject.SetActive(active);
            provider.RightSaber.gameObject.SetActive(active);
            provider.Head.gameObject.SetActive(active);
        }

        #endregion

        #region Saber Prefab

        private static readonly int colorProperty = Shader.PropertyToID("_CoreColor");

        private static VRController InstantiateSaber(Transform parent) {
            var saber = Object.Instantiate(BundleLoader.SaberPrefab, parent, false);
            var meshRenderers = saber.GetComponentsInChildren<MeshRenderer>();

            var blade = meshRenderers.First(static x => x.name == "Blade");
            var handle = meshRenderers.First(static x => x.name == "Handle");
            var bladeMaterial = InstantiateAndReplaceMaterial(blade);
            var handleMaterial = InstantiateAndReplaceMaterial(handle);

            var controller = saber.AddComponent<CustomVRController>();
            controller.enabled = false;
            controller.bladeMaterial = bladeMaterial;
            controller.handleMaterial = handleMaterial;
            return controller;
        }

        private static Material InstantiateAndReplaceMaterial(MeshRenderer renderer) {
            var originalMaterial = renderer.material;
            var copiedMaterial = Object.Instantiate(originalMaterial);
            renderer.material = copiedMaterial;
            return copiedMaterial;
        }

        private static void RefreshSabers(IVRControllersProvider provider, Color color) {
            RefreshSaber(provider.LeftSaber, color);
            RefreshSaber(provider.RightSaber, color);
        }

        private static void RefreshSaber(VRController controller, Color color) {
            var customController = (CustomVRController)controller;
            customController.bladeMaterial!.SetColor(colorProperty, color);
            customController.handleMaterial!.SetColor(colorProperty, color);
        }

        #endregion
    }
}