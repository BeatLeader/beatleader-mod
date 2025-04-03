using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerBattleRoyaleSabers : IVirtualPlayerBody {
        #region Pool

        public class Pool : MemoryPool<IVirtualPlayer, VirtualPlayerBattleRoyaleSabers> {
            public override void OnCreated(VirtualPlayerBattleRoyaleSabers item) {
                item.Initialize();
            }

            public override void Reinitialize(IVirtualPlayer player, VirtualPlayerBattleRoyaleSabers item) {
                item.RefreshVisuals(player);
            }
        }

        #endregion

        #region Setup

        private void Initialize() {
            _leftHandTransform = Object.Instantiate(BundleLoader.SaberPrefab, null, false).transform;
            _rightHandTransform = Object.Instantiate(BundleLoader.SaberPrefab, null, false).transform;
            
            _leftHand = _leftHandTransform.GetComponent<BattleRoyaleVRController>();
            _rightHand = _rightHandTransform.GetComponent<BattleRoyaleVRController>();
            
            _leftHand.CoreIntensity = 1f;
            _rightHand.CoreIntensity = 1f;
        }

        private void RefreshVisuals(IVirtualPlayer player) {
            var color = player.Replay.OptionalReplayData?.AccentColor ?? Color.clear;
            _leftHand.CoreColor = color;
            _rightHand.CoreColor = color;
        }

        #endregion

        #region Sabers

        private BattleRoyaleVRController _leftHand = null!;
        private BattleRoyaleVRController _rightHand = null!;
        private Transform _leftHandTransform = null!;
        private Transform _rightHandTransform = null!;

        public void ApplySettings(BattleRoyaleBodySettings basicBodySettings) {
            _leftHand.TrailLength = basicBodySettings.TrailLength;
            _rightHand.TrailLength = basicBodySettings.TrailLength;
            
            _leftHand.gameObject.SetActive(basicBodySettings.LeftSaberEnabled);
            _rightHand.gameObject.SetActive(basicBodySettings.RightSaberEnabled);
        }
        
        public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
            _leftHandTransform.SetLocalPose(leftHandPose);
            _rightHandTransform.SetLocalPose(rightHandPose);
        }

        #endregion
    }
}