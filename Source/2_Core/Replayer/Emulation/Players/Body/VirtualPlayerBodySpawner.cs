using System;
using System.Collections.Generic;
using System.Linq;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    /// <summary>
    /// An adapter class which combines sabers and avatar abstractions 
    /// </summary>
    internal class VirtualPlayerBodySpawner : MonoBehaviour, IVirtualPlayerBodySpawner {
        #region Adapter

        private record VirtualPlayerBodyAdapter(
            IControllableVirtualPlayerBody Avatar,
            IControllableVirtualPlayerBody Sabers
        ) : IControllableVirtualPlayerBody {
            public void RefreshVisuals() {
                Avatar.RefreshVisuals();
                Sabers.RefreshVisuals();
            }

            public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
                Avatar.ApplyPose(headPose, leftHandPose, rightHandPose);
                Sabers.ApplyPose(headPose, leftHandPose, rightHandPose);
            }
        }

        #endregion

        #region Injection

        [Inject] private readonly VirtualPlayerAvatarSpawnerBase _avatarSpawner = null!;
        [Inject] private readonly VirtualPlayerSabersSpawnerBase _sabersSpawner = null!;
        [Inject] private readonly ReplayLaunchData _replayLaunchData = null!;

        #endregion

        #region Models

        public IReadOnlyList<IVirtualPlayerBodyModel> BodyModels { get; private set; } = null!;
        public IReadOnlyDictionary<IVirtualPlayerBodyModel, IVirtualPlayerBodyConfig> BodyConfigs { get; private set; } = null!;

        private IVirtualPlayerBodyModel _primaryModel = null!;
        private IVirtualPlayerBodyModel _model = null!;

        private IVirtualPlayerBodyConfig _primaryConfig = null!;
        private IVirtualPlayerBodyConfig _config = null!;

        private void Awake() {
            _model = MergeModels(
                "Default",
                _avatarSpawner.Model,
                _sabersSpawner.Model
            );
            _primaryModel = MergeModels(
                "Primary",
                _avatarSpawner.PrimaryModel,
                _sabersSpawner.PrimaryModel
            );

            _config = GetConfigByModel(_model);
            _primaryConfig = GetConfigByModel(_primaryModel);

            _config.ConfigUpdatedEvent += HandleConfigUpdated;
            _primaryConfig.ConfigUpdatedEvent += HandlePrimaryConfigUpdated;
            
            var isBattleRoyale = _replayLaunchData.IsBattleRoyale;
            BodyModels = isBattleRoyale ?
                new[] { _model, _primaryModel } :
                new[] { _primaryModel };
            BodyConfigs = isBattleRoyale ? new Dictionary<IVirtualPlayerBodyModel, IVirtualPlayerBodyConfig> {
                { _model, _config },
                { _primaryModel, _primaryConfig }
            } : new Dictionary<IVirtualPlayerBodyModel, IVirtualPlayerBodyConfig> {
                { _primaryModel, _primaryConfig }
            };
            
            HandleConfigUpdated(null);
            HandlePrimaryConfigUpdated(null);
        }

        private void OnDestroy() {
            _config.ConfigUpdatedEvent -= HandleConfigUpdated;
            _primaryConfig.ConfigUpdatedEvent -= HandlePrimaryConfigUpdated;
        }

        private IVirtualPlayerBodyConfig GetConfigByModel(IVirtualPlayerBodyModel model) {
            var bodySettings = _replayLaunchData.Settings.BodySettings;
            var conf = bodySettings.GetConfigByNameOrNull(model.Name);
            conf ??= new SerializableVirtualPlayerBodyConfig(model);
            bodySettings.AddOrUpdateConfig(model, conf);
            return conf;
        }

        private static IVirtualPlayerBodyModel MergeModels(
            string name,
            IVirtualPlayerBodyModel model1,
            IVirtualPlayerBodyModel model2
        ) {
            return new VirtualPlayerBodyModel(
                name,
                model1.Parts.Concat(model2.Parts).ToArray()
            );
        }

        #endregion

        #region Spawn & Despawn

        public IControllableVirtualPlayerBody SpawnBody(IVirtualPlayersManager playersManager, IVirtualPlayerBase player) {
            var avatar = _avatarSpawner.Spawn(playersManager, player);
            var sabers = _sabersSpawner.Spawn(playersManager, player);
            return new VirtualPlayerBodyAdapter(avatar, sabers);
        }

        public void DespawnBody(IVirtualPlayerBody body) {
            if (body is not VirtualPlayerBodyAdapter castedBody) {
                throw new InvalidOperationException("Unable to despawn a body which does not belong to the pool");
            }
            _avatarSpawner.Despawn(castedBody.Avatar);
            _sabersSpawner.Despawn(castedBody.Sabers);
        }

        #endregion

        #region Callbacks

        private void HandlePrimaryConfigUpdated(IVirtualPlayerBodyPartConfig? config) {
            UpdateConfig(true, _primaryConfig);
        }

        private void HandleConfigUpdated(IVirtualPlayerBodyPartConfig? config) {
            UpdateConfig(false, _config);
        }

        private void UpdateConfig(bool primary, IVirtualPlayerBodyConfig config) {
            _avatarSpawner.ApplyModelConfig(primary, config);
            _sabersSpawner.ApplyModelConfig(primary, config);
        }

        #endregion
    }
}