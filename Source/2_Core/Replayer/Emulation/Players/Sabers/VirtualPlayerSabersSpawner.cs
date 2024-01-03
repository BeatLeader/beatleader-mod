using System;
using System.Collections.Generic;
using BeatLeader.Models;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerSabersSpawner : IVirtualPlayerSabersSpawner {
        #region Sabers

        private class Sabers : IVirtualPlayerSabers, IHandVRControllersProvider {
            public Sabers(
                VirtualPlayerSabersSpawner spawner,
                BattleRoyaleVRController leftHand,
                BattleRoyaleVRController rightHand
            ) {
                _leftHand = leftHand;
                _rightHand = rightHand;
                _sabersSpawner = spawner;
                ControllersProvider = this;
                ApplyAlpha(1f);
            }

            #region ControllersProvider

            public VRController LeftHand => _leftHand;
            public VRController RightHand => _rightHand;

            #endregion

            #region Sabers

            public IHandVRControllersProvider ControllersProvider { get; private set; }
            public bool HasAlphaSupport { get; private set; }

            private readonly VirtualPlayerSabersSpawner _sabersSpawner;
            private readonly BattleRoyaleVRController _leftHand;
            private readonly BattleRoyaleVRController _rightHand;
            private bool _originalSabersBorrowed;

            public void SetActive(bool active) {
                _leftHand.gameObject.SetActive(active);
                _rightHand.gameObject.SetActive(active);
            }

            public void ApplyConfig(VirtualPlayerSabersConfig config) {
                HasAlphaSupport = !config.Primary;
                if (HasAlphaSupport) ApplyAlpha(config.Alpha);
                var useOriginalSabers = config.Primary;
                
                SetActive(!useOriginalSabers);
                if (useOriginalSabers) {
                    if (_originalSabersBorrowed) return;
                    ControllersProvider = _sabersSpawner.BorrowOriginalSabers();
                } else {
                    if (!_originalSabersBorrowed) return;
                    _sabersSpawner.ReturnOriginalSabers();
                    ControllersProvider = this;
                }
                _originalSabersBorrowed = useOriginalSabers;
            }

            public void ApplyPlayer(IVirtualPlayerBase player) {
                var color = player.Replay.ReplayData.Player?.AccentColor ?? Color.white;
                _leftHand.CoreColor = color;
                _rightHand.CoreColor = color;
            }

            private void ApplyAlpha(float alpha) {
                _leftHand.CoreIntensity = alpha;
                _rightHand.CoreIntensity = alpha;
            }

            #endregion
        }

        #endregion

        #region Injection

        [Inject] private readonly OriginalVRControllersProvider _originalVRControllersProvider = null!;
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjectsProvider = null!;

        #endregion

        #region Original Sabers

        private bool _originalSabersUnavailable;

        private IHandVRControllersProvider BorrowOriginalSabers() {
            if (_originalSabersUnavailable) {
                throw new InvalidOperationException("Unable to access sabers while they are borrowed");
            }
            _originalSabersUnavailable = true;
            return _originalVRControllersProvider;
        }

        private void ReturnOriginalSabers() {
            _originalSabersUnavailable = false;
        }

        #endregion

        #region Spawn & Despawn

        private readonly Stack<Sabers> _availableProviders = new();
        private readonly HashSet<Sabers> _allProviders = new();

        public IVirtualPlayerSabers SpawnSabers(IVirtualPlayerBase player) {
            Sabers sabers;
            if (_availableProviders.Count is 0) {
                sabers = SpawnSabersInternal();
                _allProviders.Add(sabers);
            } else {
                sabers = _availableProviders.Pop();
            }
            sabers.ApplyPlayer(player);
            sabers.SetActive(true);
            return sabers;
        }

        public void DespawnSabers(IVirtualPlayerSabers sabers) {
            if (sabers is not Sabers castedSabers || !_allProviders.Contains(castedSabers)) {
                throw new InvalidOperationException("Unable to despawn provider which does not belong to the pool");
            }
            castedSabers.SetActive(false);
            _availableProviders.Push(castedSabers);
        }

        private Sabers SpawnSabersInternal() {
            var transform = new GameObject("SabersContainer").transform;
            transform.SetParent(_extraObjectsProvider.VRGameCore, false);

            var left = InstantiateSaber(transform);
            var right = InstantiateSaber(transform);
            return new(this, left, right);
        }

        #endregion

        #region Prefab

        private static BattleRoyaleVRController InstantiateSaber(Transform parent) {
            return InstantiateVRControllerPrefab(BundleLoader.SaberPrefab, parent);
        }

        private static BattleRoyaleVRController InstantiateVRControllerPrefab(GameObject prefab, Transform parent) {
            var head = Object.Instantiate(prefab, parent, false);
            var controller = head.GetComponent<BattleRoyaleVRController>();
            return controller;
        }

        #endregion
    }
}