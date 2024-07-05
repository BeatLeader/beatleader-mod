using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    //TODO: avatar opacity support
    internal class VirtualPlayerAvatarBody : IVirtualPlayerBodyComponent {
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
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;

        #endregion

        #region Setup

        public bool UsesPrimaryModel { get; private set; }

        private IVirtualPlayerBase _player = null!;
        private AvatarLoader _avatarLoader = null!;
        private AvatarPartsModel _avatarPartsModel = null!;
        private AvatarData? _avatarData;

        private void Setup() {
            _avatarLoader = _zenjectMenuResolver.Resolve<AvatarLoader>();
            _avatarPartsModel = _zenjectMenuResolver.Resolve<AvatarPartsModel>();
            _avatarController = _avatarLoader.CreateAvatar();
            _avatarController.PlayAnimation = false;
            _avatarController.transform.localScale = Vector3.one;
            LoadBody();
        }

        private void RefreshAvatarVisuals(IVirtualPlayerBase player) {
            _player = player;
            var replay = player.Replay;
            _player = player;
            _avatarData = replay.OptionalReplayData?.AvatarData;
            if (_avatarData == null) {
                _avatarData = new();
                var playerId = replay.ReplayData.Player!.Id;
                AvatarUtils.RandomizeAvatarByPlayerId(playerId, _avatarData, _avatarPartsModel);
            }
            _avatarController.VisualController.UpdateAvatarVisual(_avatarData);
        }

        #endregion

        #region VirtualPlayerBody

        public void RefreshVisuals() {
            UsesPrimaryModel = _playersManager.PrimaryPlayer == _player;
        }

        public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
            _headTransform.SetLocalPose(headPose);
            _leftHandTransform.SetLocalPose(leftHandPose);
            _rightHandTransform.SetLocalPose(rightHandPose);
        }

        public void ApplyConfig(IVirtualPlayerBodyConfig config) {
            foreach (var (id, part) in config.BodyParts) {
                var trans = id switch {
                    "BATTLE_ROYALE_AVATAR_HEAD" => _headTransform,
                    "BATTLE_ROYALE_AVATAR_BODY" => _bodyTransform,
                    "BATTLE_ROYALE_AVATAR_LEFT_HAND" => _leftHandTransform,
                    "BATTLE_ROYALE_AVATAR_RIGHT_HAND" => _rightHandTransform,
                    _ => default
                };
                trans?.gameObject.SetActive(part.Active);
            }
        }

        #endregion

        #region BodyModel

        private static readonly List<IVirtualPlayerBodyPartModel> bodyPartModels = new() {
            new VirtualPlayerBodyPartModel("Head", "BATTLE_ROYALE_AVATAR_HEAD", null, BodyNode.Head, false),
            new VirtualPlayerBodyPartModel("Body", "BATTLE_ROYALE_AVATAR_BODY", null, BodyNode.Unknown, false),
            new VirtualPlayerBodyPartModel("Left Hand", "BATTLE_ROYALE_AVATAR_LEFT_HAND", "Hands", BodyNode.LeftHand, false),
            new VirtualPlayerBodyPartModel("Right Hand", "BATTLE_ROYALE_AVATAR_RIGHT_HAND", "Hands", BodyNode.RightHand, false),
        };

        public static readonly IVirtualPlayerBodyModel BodyModel = new VirtualPlayerBodyModel("Avatar", bodyPartModels);

        #endregion

        #region Body

        private AvatarController _avatarController = null!;
        private Transform _headTransform = null!;
        private Transform _leftHandTransform = null!;
        private Transform _rightHandTransform = null!;
        private Transform _bodyTransform = null!;

        private void LoadBody() {
            var avatarPoseController = _avatarController.PoseController;
            //TODO: asm pub
            _headTransform = avatarPoseController.GetField<Transform, AvatarPoseController>("_headTransform");
            _leftHandTransform = avatarPoseController.GetField<Transform, AvatarPoseController>("_leftHandTransform");
            _rightHandTransform = avatarPoseController.GetField<Transform, AvatarPoseController>("_rightHandTransform");
            _bodyTransform = avatarPoseController.GetField<Transform, AvatarPoseController>("_bodyTransform");
        }

        #endregion
    }
}