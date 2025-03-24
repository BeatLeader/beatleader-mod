using BeatLeader.Models;
using BeatLeader.Utils;
using BeatSaber.BeatAvatarSDK;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerAvatarBody : IVirtualPlayerBody {
        #region Pool

        public class Pool : MemoryPool<IVirtualPlayer, VirtualPlayerAvatarBody> {
            public override void OnCreated(VirtualPlayerAvatarBody item) {
                item.Initialize();
            }

            public override void Reinitialize(IVirtualPlayer avatarData, VirtualPlayerAvatarBody item) {
                item.RefreshVisuals(avatarData);
            }
        }

        #endregion

        #region Injection

        [Inject] private readonly ZenjectMenuResolver _zenjectMenuResolver = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;
        [Inject] private readonly ReplayerExtraObjectsProvider _extraObjectsProvider = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        #endregion

        #region Setup

        private BeatAvatarController _avatarController = null!;
        private Transform _headTransform = null!;
        private Transform _leftHandTransform = null!;
        private Transform _rightHandTransform = null!;
        private Transform _bodyTransform = null!;

        private IVirtualPlayer _player = null!;
        private BeatAvatarLoader _beatAvatarLoader = null!;
        private AvatarData? _avatarData;

        private void Initialize() {
            _beatAvatarLoader = _zenjectMenuResolver.Resolve<BeatAvatarLoader>();
            _avatarController = _beatAvatarLoader.CreateGameplayAvatar(_extraObjectsProvider.VRGameCore);
            LoadBody();
        }

        private void LoadBody() {
            var avatarPoseController = _avatarController.PoseController;
            _headTransform = avatarPoseController._headTransform;
            _leftHandTransform = avatarPoseController._leftHandTransform;
            _rightHandTransform = avatarPoseController._rightHandTransform;
            _bodyTransform = avatarPoseController._bodyTransform;
        }

        #endregion

        #region VirtualPlayerBody

        private void RefreshVisuals(IVirtualPlayer player) {
            _player = player;
            _avatarData = _player.Replay.OptionalReplayData?.AvatarData ?? AvatarUtils.DefaultAvatarData;

            _avatarController.SetVisuals(_avatarData);
        }

        public void ApplySettings(BasicBodySettings basicBodySettings) {
            
        }

        public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
            _headTransform.SetLocalPose(headPose);
            _leftHandTransform.SetLocalPose(leftHandPose);
            _rightHandTransform.SetLocalPose(rightHandPose);
            _avatarController.PoseController.UpdateBodyPosition();
        }

        #endregion
    }
}