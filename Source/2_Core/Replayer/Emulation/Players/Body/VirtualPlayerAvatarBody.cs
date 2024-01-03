using System;
using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    //TODO: avatar opacity support
    internal class VirtualPlayerAvatarBody : IVirtualPlayerBody, IVRControllersProvider {
        #region Pool

        public class Pool : MemoryPool<IVirtualPlayerBase, VirtualPlayerAvatarBody> {
            protected override void OnCreated(VirtualPlayerAvatarBody item) {
                item.Setup();
            }

            protected override void Reinitialize(IVirtualPlayerBase playerBase, VirtualPlayerAvatarBody item) {
                item.RefreshAvatarVisuals(playerBase);
            }
        }

        #endregion

        #region Injection

        [Inject] private readonly ZenjectMenuResolver _zenjectMenuResolver = null!;

        #endregion

        #region Setup

        private AvatarLoader _avatarLoader = null!;
        private AvatarPartsModel _avatarPartsModel = null!;
        private readonly AvatarData _avatarData = new();

        private void Setup() {
            _avatarLoader = _zenjectMenuResolver.Resolve<AvatarLoader>();
            _avatarPartsModel = _zenjectMenuResolver.Resolve<AvatarPartsModel>();
            _avatarController = _avatarLoader.CreateAvatar();
            _avatarController.PlayAnimation = false;
            _avatarController.containerTransform.localScale = Vector3.one;
            LoadBody();
        }

        private void RefreshAvatarVisuals(IVirtualPlayerBase player) {
            var playerId = player.Replay.ReplayData.Player!.Id;
            AvatarUtils.RandomizeAvatarByPlayerId(playerId, _avatarData, _avatarPartsModel);
            _avatarController.visualController.UpdateAvatarVisual(_avatarData);
        }

        #endregion

        #region VirtualPlayerBody

        public IVRControllersProvider ControllersProvider => this;

        public void ApplyConfig(VirtualPlayerBodyConfig config) {
            foreach (var part in config.AvailableBodyParts) {
                var trans = part.Id switch {
                    "BATTLE_ROYALE_AVATAR_HEAD" => _headTransform,
                    "BATTLE_ROYALE_AVATAR_BODY" => _bodyTransform,
                    "BATTLE_ROYALE_AVATAR_LEFT_HAND" => _leftHandTransform,
                    "BATTLE_ROYALE_AVATAR_RIGHT_HAND" => _rightHandTransform,
                    _ => throw new ArgumentOutOfRangeException()
                };
                var isActive = config.IsPartActive(part);
                trans.gameObject.SetActive(isActive);
            }
        }

        #endregion

        #region BodyModel

        private record BodyPartModel(string Name, string Id) : IVirtualPlayerBodyPartModel {
            public IReadOnlyCollection<IVirtualPlayerBodyPartSegmentModel> Segments { get; } = Array.Empty<IVirtualPlayerBodyPartSegmentModel>();
        }

        private record AvatarBodyModel(IReadOnlyCollection<IVirtualPlayerBodyPartModel> Parts) : IVirtualPlayerBodyModel;

        private static readonly List<IVirtualPlayerBodyPartModel> bodyPartModels = new() {
            new BodyPartModel("Head", "BATTLE_ROYALE_AVATAR_HEAD"),
            new BodyPartModel("Body", "BATTLE_ROYALE_AVATAR_BODY"),
            new BodyPartModel("Left Hand", "BATTLE_ROYALE_AVATAR_LEFT_HAND"),
            new BodyPartModel("Right Hand", "BATTLE_ROYALE_AVATAR_RIGHT_HAND"),
        };

        public static IVirtualPlayerBodyModel BodyModel { get; } = new AvatarBodyModel(bodyPartModels);

        #endregion

        #region Body

        public VRController LeftHand { get; private set; } = null!;
        public VRController RightHand { get; private set; } = null!;
        public VRController Head { get; private set; } = null!;

        private AvatarController _avatarController = null!;
        private Transform _headTransform = null!;
        private Transform _leftHandTransform = null!;
        private Transform _rightHandTransform = null!;
        private Transform _bodyTransform = null!;

        private void LoadBody() {
            var avatarPoseController = _avatarController.poseController;
            //TODO: asm pub
            _headTransform = avatarPoseController.GetField<Transform, AvatarPoseController>("_headTransform");
            _leftHandTransform = avatarPoseController.GetField<Transform, AvatarPoseController>("_leftHandTransform");
            _rightHandTransform = avatarPoseController.GetField<Transform, AvatarPoseController>("_rightHandTransform");
            _bodyTransform = avatarPoseController.GetField<Transform, AvatarPoseController>("_bodyTransform");

            Head = AddVRController(_headTransform);
            LeftHand = AddVRController(_leftHandTransform);
            RightHand = AddVRController(_rightHandTransform);
        }

        private static VRController AddVRController(Transform trans) {
            var controller = trans.gameObject.AddComponent<VRController>();
            controller.enabled = false;
            return controller;
        }

        #endregion
    }
}