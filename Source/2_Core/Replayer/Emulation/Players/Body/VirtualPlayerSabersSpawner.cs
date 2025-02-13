using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerSabersSpawner : VirtualPlayerSabersSpawnerBase {
        #region Sabers

        private class Sabers : IVirtualPlayerBodyComponent {
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
                var color = player.Replay.OptionalReplayData?.AccentColor ?? Color.clear;
                _leftHand.CoreColor = color;
                _rightHand.CoreColor = color;
                _player = player;
            }

            public void ApplyConfig(IVirtualPlayerBodyConfig config) {
                if (config.BodyParts.TryGetValue("LEFT_SABER", out var leftHandConfig)) {
                    ApplySaberConfig(true, leftHandConfig);
                }
                if (config.BodyParts.TryGetValue("RIGHT_SABER", out var rightHandConfig)) {
                    ApplySaberConfig(false, rightHandConfig);
                }
            }

            private void ApplySaberConfig(bool left, IVirtualPlayerBodyPartConfig config) {
                if (_usesOriginalSabers) {
                    var saber = left ? _originalControllers!.LeftHand : _originalControllers!.RightHand;
                    saber.gameObject.SetActive(config.Active);
                } else {
                    var saber = left ? _leftHand : _rightHand;
                    saber.CoreIntensity = config.Alpha;
                    saber.gameObject.SetActive(config.Active);
                }
            }

            #endregion

            #region Sabers

            private IVirtualPlayerBase _player = null!;
            private readonly IVirtualPlayersManager _playersManager;
            private readonly VirtualPlayerSabersSpawner _sabersSpawner;

            private IHandVRControllersProvider? _originalControllers;
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
                    _originalControllers = _sabersSpawner.BorrowOriginalSabers(this);
                } else {
                    if (!_usesOriginalSabers) return;
                    _sabersSpawner.ReturnOriginalSabers();
                    _originalControllers = null;
                }
                _usesOriginalSabers = useOriginalSabers;
            }

            public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
                Transform leftHandTransform;
                Transform rightHandTransform;
                if (_usesOriginalSabers) {
                    leftHandTransform = _originalControllers!.LeftHand.transform;
                    rightHandTransform = _originalControllers!.RightHand.transform;
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
                new VirtualPlayerBodyPartModel("Left Saber", "LEFT_SABER", "Sabers", BodyNode.LeftHand, false),
                new VirtualPlayerBodyPartModel("Right Saber", "RIGHT_SABER", "Sabers", BodyNode.RightHand, false)
            }
        );

        private static readonly VirtualPlayerBodyModel model = new(
            "Battle Royale Sabers",
            new[] {
                new VirtualPlayerBodyPartModel("Left Saber", "LEFT_SABER", "Sabers", BodyNode.LeftHand, true),
                new VirtualPlayerBodyPartModel("Right Saber", "RIGHT_SABER", "Sabers", BodyNode.RightHand, true)
            }
        );

        #endregion

        #region Config

        protected override void ApplyPrimaryConfig(IVirtualPlayerBodyConfig config) {
            _sabersKeeper?.ApplyConfig(config);
        }

        #endregion

        #region Spawn & Despawn

        private readonly Stack<Sabers> _availableProviders = new();

        protected override IVirtualPlayerBodyComponent SpawnInternal(IVirtualPlayersManager playersManager, IVirtualPlayerBase player) {
            var sabers = _availableProviders.Count is 0 ?
                SpawnSabersInternal(playersManager) :
                _availableProviders.Pop();
            sabers.ApplyPlayer(player);
            sabers.SetActive(true);
            return sabers;
        }

        protected override void DespawnInternal(IVirtualPlayerBodyComponent body) {
            var sabers = (Sabers)body;
            sabers.SetActive(false);
            _availableProviders.Push(sabers);
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