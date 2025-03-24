using System;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerBodySpawner : MonoBehaviour, IVirtualPlayerBodySpawner {
        #region Adapter

        private record VirtualPlayerBodyAdapter(
            IVirtualPlayerBody Avatar,
            IVirtualPlayerBody Sabers,
            bool UsesPrimarySabers
        ) : IVirtualPlayerBody {
            public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
                Avatar.ApplyPose(headPose, leftHandPose, rightHandPose);
                Sabers.ApplyPose(headPose, leftHandPose, rightHandPose);
            }
        }

        #endregion

        #region Setup

        [Inject] private readonly VirtualPlayerAvatarBody.Pool _avatarsPool = null!;
        [Inject] private readonly VirtualPlayerBattleRoyaleSabers.Pool _sabersPool = null!;
        [Inject] private readonly VirtualPlayerGameSabers _gameSabers = null!;
        [Inject] private readonly ReplayLaunchData _replayLaunchData = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;

        private void Awake() {
            _replayLaunchData.Settings.BodySettings.SettingsUpdatedEvent += HandleBodySettingsUpdated;
        }

        private void OnDestroy() {
            _replayLaunchData.Settings.BodySettings.SettingsUpdatedEvent -= HandleBodySettingsUpdated;
        }

        #endregion

        #region Sabers

        private bool _sabersBorrowed;

        private IVirtualPlayerBody SpawnOrBorrowSabers(IVirtualPlayer player, bool primary) {
            if (primary && _sabersBorrowed) {
                throw new InvalidOperationException("Game sabers are already borrowed");
            }

            if (primary) {
                _sabersBorrowed = true;
            }
            
            return primary ? _gameSabers : _sabersPool.Spawn(player);
        }

        private void ReleaseSabers(IVirtualPlayerBody sabers, bool primary) {
            if (primary) {
                _sabersBorrowed = false;
                return;
            }
            
            _sabersPool.Despawn((VirtualPlayerBattleRoyaleSabers)sabers);
        }

        #endregion

        #region Impl

        public bool BodyHeadsVisible { get; set; }
        
        public IVirtualPlayerBody SpawnBody(IVirtualPlayer player) {
            var primary = player == _playersManager.PrimaryPlayer;

            var sabers = SpawnOrBorrowSabers(player, primary);
            var avatar = _avatarsPool.Spawn(player);

            return new VirtualPlayerBodyAdapter(avatar, sabers, primary);
        }

        public void DespawnBody(IVirtualPlayerBody body) {
            if (body is not VirtualPlayerBodyAdapter castedBody) {
                throw new InvalidOperationException("Unable to despawn a body which does not belong to the pool");
            }
            
            _avatarsPool.Despawn((VirtualPlayerAvatarBody)castedBody.Avatar);
            ReleaseSabers(castedBody.Sabers, castedBody.UsesPrimarySabers);
        }

        #endregion
        
        #region Config

        private void HandleBodySettingsUpdated(BodySettings settings) {
            //TODO: implement settings
        }
        
        #endregion
    }
}