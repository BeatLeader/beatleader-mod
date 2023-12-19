using System;
using System.Collections.Generic;
using BeatLeader.Models;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BeatLeader.Replayer.Emulation {
    internal class VRControllersSpawner : IVRControllersSpawner {
        #region Injection

        [Inject] private readonly OriginalVRControllersProvider _originalVRControllersProvider = null!;
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjectsProvider = null!;

        #endregion

        #region Spawn & Despawn

        public float ControllersIntensity {
            get => _additionalControllersIntensity;
            set {
                _additionalControllersIntensity = value;
                foreach (var provider in _allProviders) {
                    RefreshControllersIntensity(provider, value);
                }
            }
        }
        
        private readonly Stack<IVRControllersProvider> _availableProviders = new();
        private readonly HashSet<IVRControllersProvider> _allProviders = new();
        private bool _primaryControllerUnavailable;
        private float _additionalControllersIntensity = 0.2f;
        
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
                0 => SpawnControllersInternal(),
                _ => _availableProviders.Pop()
            };
            var color = player.Replay.ReplayData.Player?.AccentColor ?? Color.white;
            RefreshControllersColor(provider, color);
            SetControllersActive(provider, true);
            return provider;
        }

        private IVRControllersProvider SpawnControllersInternal() {
            var transform = new GameObject("VRControllersContainer").transform;
            transform.SetParent(_extraObjectsProvider.VRGameCore, false);

            var left = InstantiateSaber(transform);
            var right = InstantiateSaber(transform);
            var head = InstantiateHead(transform);

            var provider = new GenericVRControllersProvider(left, right, head);
            _allProviders.Add(provider);
            
            RefreshControllersIntensity(provider, ControllersIntensity);
            return provider;
        }

        public void DespawnControllers(IVRControllersProvider provider) {
            if (provider.Equals(_originalVRControllersProvider)) {
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

        #endregion

        #region Controllers

        private static void SetControllersActive(IVRControllersProvider provider, bool active) {
            provider.LeftSaber.gameObject.SetActive(active);
            provider.RightSaber.gameObject.SetActive(active);
            provider.Head.gameObject.SetActive(active);
        }

        private static void RefreshControllersColor(IVRControllersProvider provider, Color color) {
            RefreshControllerColor(provider.LeftSaber, color);
            RefreshControllerColor(provider.RightSaber, color);
            RefreshControllerColor(provider.Head, color);
        }
        
        private static void RefreshControllersIntensity(IVRControllersProvider provider, float intensity) {
            RefreshControllerIntensity(provider.LeftSaber, intensity);
            RefreshControllerIntensity(provider.RightSaber, intensity);
            RefreshControllerIntensity(provider.Head, intensity);
        }

        private static void RefreshControllerColor(VRController controller, Color color) {
            var modelController = (BattleRoyaleVRController)controller;
            modelController.CoreColor = color;
        }
        
        private static void RefreshControllerIntensity(VRController controller, float intensity) {
            var modelController = (BattleRoyaleVRController)controller;
            modelController.CoreIntensity = intensity;
        }

        #endregion

        #region Prefabs
        
        private static VRController InstantiateHead(Transform parent) {
            return InstantiateVRControllerPrefab(BundleLoader.HeadPrefab, parent);
        }
        
        private static VRController InstantiateSaber(Transform parent) {
            return InstantiateVRControllerPrefab(BundleLoader.SaberPrefab, parent);
        }

        private static VRController InstantiateVRControllerPrefab(GameObject prefab, Transform parent) {
            var head = Object.Instantiate(prefab, parent, false);
            var controller = head.GetComponent<BattleRoyaleVRController>();
            return controller;
        }
        
        #endregion
    }
}