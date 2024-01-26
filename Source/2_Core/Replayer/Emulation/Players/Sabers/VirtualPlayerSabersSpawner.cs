using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerSabersSpawner : VirtualPlayerBodyComponentSpawnerBase, IVirtualPlayerSabersSpawner {
        #region Sabers

        private class Sabers : IVirtualPlayerSabers {
            public Sabers(
                IVirtualPlayersManager playersManager,
                VirtualPlayerSabersSpawner spawner,
                BattleRoyaleVRController leftHand,
                BattleRoyaleVRController rightHand
            ) {
                _playersManager = playersManager;
                _leftHand = leftHand;
                _leftHandTransform = leftHand.transform;
                _rightHand = rightHand;
                _rightHandTransform = rightHand.transform;
                _sabersSpawner = spawner;
                _leftHand.CoreIntensity = 1f;
                _rightHand.CoreIntensity = 1f;
            }

            #region Setup

            public bool UsesPrimaryModel => _usesOriginalSabers;

            public void ApplyPlayer(IVirtualPlayerBase player) {
                var color = player.Replay.ReplayData.Player?.AccentColor ?? Color.white;
                _leftHand.CoreColor = color;
                _rightHand.CoreColor = color;
                _player = player;
            }

            public void ApplyConfig(VirtualPlayerBodyConfig config) {
                foreach (var part in config.AvailableBodyParts) {
                    GameObject? gameObject;
                    if (!_usesOriginalSabers) {
                        var controller = part.Id switch {
                            "LEFT_SABER" => _leftHand,
                            "RIGHT_SABER" => _rightHand,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        controller.CoreIntensity = part.Alpha;
                        gameObject = controller.gameObject;
                    } else {
                        var controller = part.Id switch {
                            "LEFT_SABER" => _originalControllersProvider!.LeftHand,
                            "RIGHT_SABER" => _originalControllersProvider!.RightHand,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        gameObject = controller.gameObject;
                    }
                    var active = config.IsPartActive(part);
                    gameObject.SetActive(active);
                }
            }

            #endregion

            #region Sabers

            private IVirtualPlayerBase _player = null!;
            private readonly IVirtualPlayersManager _playersManager;
            private readonly VirtualPlayerSabersSpawner _sabersSpawner;

            private IHandVRControllersProvider? _originalControllersProvider;
            private bool _usesOriginalSabers;

            private readonly BattleRoyaleVRController _leftHand;
            private readonly Transform _leftHandTransform;
            private readonly BattleRoyaleVRController _rightHand;
            private readonly Transform _rightHandTransform;

            public void SetActive(bool active) {
                _leftHand.gameObject.SetActive(active);
                _rightHand.gameObject.SetActive(active);
            }

            public void RefreshVisuals() {
                var useOriginalSabers = _playersManager.PrimaryPlayer == _player;
                SetActive(!useOriginalSabers);
                if (useOriginalSabers) {
                    if (_usesOriginalSabers) return;
                    _originalControllersProvider = _sabersSpawner.BorrowOriginalSabers(this);
                } else {
                    if (!_usesOriginalSabers) return;
                    _sabersSpawner.ReturnOriginalSabers();
                    _originalControllersProvider = null;
                }
                _usesOriginalSabers = useOriginalSabers;
            }

            public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
                Transform leftHandTransform;
                Transform rightHandTransform;
                if (_usesOriginalSabers) {
                    leftHandTransform = _originalControllersProvider!.LeftHand.transform;
                    rightHandTransform = _originalControllersProvider!.RightHand.transform;
                } else {
                    leftHandTransform = _leftHandTransform;
                    rightHandTransform = _rightHandTransform;
                }
                leftHandTransform.SetLocalPose(leftHandPose);
                rightHandTransform.SetLocalPose(rightHandPose);
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
        private Sabers? _sabersKeeper;

        private IHandVRControllersProvider BorrowOriginalSabers(Sabers keeper) {
            if (_originalSabersUnavailable) {
                throw new InvalidOperationException("Unable to access sabers while they are borrowed");
            }
            _originalSabersUnavailable = true;
            _sabersKeeper = keeper;
            return _originalVRControllersProvider;
        }

        private void ReturnOriginalSabers() {
            _originalSabersUnavailable = false;
            _sabersKeeper = null;
        }

        #endregion

        #region Model

        public override IVirtualPlayerBodyModel PrimaryModel => primaryModel;
        public override IVirtualPlayerBodyModel Model => model;

        private static readonly VirtualPlayerBodyModel primaryModel = new(
            "Base Game Sabers",
            new[] {
                new VirtualPlayerBodyPartModel("Left Saber", "LEFT_SABER", "Sabers", false),
                new VirtualPlayerBodyPartModel("Right Saber", "RIGHT_SABER", "Sabers", false)
            }
        );

        private static readonly VirtualPlayerBodyModel model = new(
            "Battle Royale Sabers",
            new[] {
                new VirtualPlayerBodyPartModel("Left Saber", "LEFT_SABER", "Sabers", true),
                new VirtualPlayerBodyPartModel("Right Saber", "RIGHT_SABER", "Sabers", true)
            }
        );

        #endregion

        #region Config

        protected override IEnumerable<IVirtualPlayerBodyComponent> SpawnedBodyComponents => _spawnedProviders;

        protected override void ApplyPrimaryConfig(VirtualPlayerBodyConfig config) {
            _sabersKeeper!.ApplyConfig(config);
        }

        #endregion

        #region Spawn & Despawn

        private readonly Stack<Sabers> _availableProviders = new();
        private readonly HashSet<Sabers> _spawnedProviders = new();

        public IVirtualPlayerSabers SpawnSabers(IVirtualPlayersManager playersManager, IVirtualPlayerBase player) {
            Sabers sabers;
            if (_availableProviders.Count is 0) {
                sabers = SpawnSabersInternal(playersManager);
                _spawnedProviders.Add(sabers);
            } else {
                sabers = _availableProviders.Pop();
            }
            sabers.ApplyPlayer(player);
            sabers.SetActive(true);
            EnhanceComponent(sabers, playersManager, player);
            return sabers;
        }

        public void DespawnSabers(IVirtualPlayerSabers sabers) {
            if (sabers is not Sabers castedSabers || !_spawnedProviders.Contains(castedSabers)) {
                throw new InvalidOperationException("Unable to despawn provider which does not belong to the pool");
            }
            castedSabers.SetActive(false);
            _spawnedProviders.Remove(castedSabers);
            _availableProviders.Push(castedSabers);
        }

        private Sabers SpawnSabersInternal(IVirtualPlayersManager playersManager) {
            var transform = new GameObject("SabersContainer").transform;
            transform.SetParent(_extraObjectsProvider.VRGameCore, false);

            var left = InstantiateSaber(transform);
            var right = InstantiateSaber(transform);
            return new(playersManager, this, left, right);
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