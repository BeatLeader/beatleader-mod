using System.Collections.Generic;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaber.AvatarCore;
using BeatSaber.BeatAvatarSDK;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerAvatarBody : IVirtualPlayerBodyComponent {
        #region Pool

        public class Pool : MemoryPool<VirtualPlayerAvatarData, VirtualPlayerAvatarBody> {
            public override void OnCreated(VirtualPlayerAvatarBody item) {
                item.Setup();
            }

            public override void Reinitialize(VirtualPlayerAvatarData avatarData, VirtualPlayerAvatarBody item) {
                item.RefreshAvatarVisuals(avatarData);
            }
        }

        #endregion

        #region Injection

        [Inject] private readonly ZenjectMenuResolver _zenjectMenuResolver = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjectsProvider = null!;

        #endregion

        #region Setup

        public bool UsesPrimaryModel { get; private set; }

        private IVirtualPlayerBase _player = null!;
        private VirtualPlayerAvatarData _playerAvatarData = null!;
        private BeatAvatarLoader _beatAvatarLoader = null!;
        private AvatarPartsModel _avatarPartsModel = null!;
        private AvatarData? _avatarData;

        private void Setup() {
            _beatAvatarLoader = _zenjectMenuResolver.Resolve<BeatAvatarLoader>();
            _avatarPartsModel = _zenjectMenuResolver.Resolve<AvatarPartsModel>();
            _avatarController = _beatAvatarLoader.CreateGameplayAvatar(_extraObjectsProvider.VRGameCore);
            LoadBody();
        }

        private void RefreshAvatarVisuals(VirtualPlayerAvatarData avatarData) {
            _player = avatarData.Player;
            _playerAvatarData = avatarData;
            var replay = _player.Replay;
            _avatarData = replay.OptionalReplayData?.AvatarData;
            if (_avatarData == null) {
                _avatarData = new();
                var playerId = replay.ReplayData.Player!.Id;
                AvatarUtils.RandomizeAvatarByPlayerId(playerId, _avatarData, _avatarPartsModel);
            }
            _avatarController.SetVisuals(_avatarData);
        }

        #endregion

        #region VirtualPlayerBody

        public void RefreshVisuals() {
            UsesPrimaryModel = _playersManager.PrimaryPlayer == _player;
            ApplyConfig(UsesPrimaryModel ? _playerAvatarData.PrimaryConfig : _playerAvatarData.DefaultConfig);
        }

        public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
            _headTransform.SetLocalPose(headPose);
            _leftHandTransform.SetLocalPose(leftHandPose);
            _rightHandTransform.SetLocalPose(rightHandPose);
            _avatarController.PoseController.UpdateBodyPosition();
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

        private BeatAvatarController _avatarController = null!;
        private Transform _headTransform = null!;
        private Transform _leftHandTransform = null!;
        private Transform _rightHandTransform = null!;
        private Transform _bodyTransform = null!;

        private void LoadBody() {
            var avatarPoseController = _avatarController.PoseController;
            _headTransform = avatarPoseController._headTransform;
            _leftHandTransform = avatarPoseController._leftHandTransform;
            _rightHandTransform = avatarPoseController._rightHandTransform;
            _bodyTransform = avatarPoseController._bodyTransform;
        }

        #endregion
    }
}