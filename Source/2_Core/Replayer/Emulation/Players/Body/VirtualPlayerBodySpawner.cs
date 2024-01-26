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
            IVirtualPlayerAvatar Avatar,
            IVirtualPlayerSabers Sabers
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

        [Inject] private readonly IVirtualPlayerAvatarSpawner _avatarSpawner = null!;
        [Inject] private readonly IVirtualPlayerSabersSpawner _sabersSpawner = null!;

        #endregion

        #region Models

        public IReadOnlyList<IVirtualPlayerBodyModel> BodyModels { get; private set; } = null!;

        private IVirtualPlayerBodyModel _primaryModel = null!;
        private IVirtualPlayerBodyModel _model = null!;

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
            BodyModels = new[] { _model, _primaryModel };
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

        public void ApplyModelConfig(IVirtualPlayerBodyModel model, VirtualPlayerBodyConfig config) {
            if (model != _model && model != _primaryModel) {
                throw new InvalidOperationException("Unable to apply config to a model which does not belong to the spawner");
            }
            var primary = model == _primaryModel;
            _avatarSpawner.ApplyModelConfig(primary, config);
            _sabersSpawner.ApplyModelConfig(primary, config);
        }

        public IControllableVirtualPlayerBody SpawnBody(IVirtualPlayersManager playersManager, IVirtualPlayerBase player) {
            var avatar = _avatarSpawner.SpawnAvatar(playersManager, player);
            var sabers = _sabersSpawner.SpawnSabers(playersManager, player);
            return new VirtualPlayerBodyAdapter(avatar, sabers);
        }

        public void DespawnBody(IVirtualPlayerBody body) {
            if (body is not VirtualPlayerBodyAdapter castedBody) {
                throw new InvalidOperationException("Unable to despawn a body which does not belong to the pool");
            }
            _avatarSpawner.DespawnAvatar(castedBody.Avatar);
            _sabersSpawner.DespawnSabers(castedBody.Sabers);
        }

        #endregion
    }
}