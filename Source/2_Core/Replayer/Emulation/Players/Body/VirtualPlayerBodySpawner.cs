using System;
using System.Collections.Generic;
using BeatLeader.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayerBodySpawner : MonoBehaviour, IVirtualPlayerBodySpawner {
        #region Adapter

        private record VirtualPlayerBodyAdapter(
            VirtualPlayerAvatarBody Avatar,
            IVirtualPlayerBody Sabers,
            bool UsesPrimarySabers
        ) : IVirtualPlayerBody {
            public void ApplyPose(Pose headPose, Pose leftHandPose, Pose rightHandPose) {
                Avatar.ApplyPose(headPose, leftHandPose, rightHandPose);
                Sabers.ApplyPose(headPose, leftHandPose, rightHandPose);
            }

            public void ApplySettings(BasicBodySettings settings) {
                var royaleSettings = settings as BattleRoyaleBodySettings;
                // a little bit of casting shenanigans
                if (UsesPrimarySabers) {
                    if (royaleSettings != null) {
                        return;
                    }
                    
                    Avatar.ApplySettings(settings);
                    ((VirtualPlayerGameSabers)Sabers).ApplySettings(settings);
                } else {
                    if (royaleSettings == null) {
                        return;
                    }

                    Avatar.ApplySettings(royaleSettings);
                    ((VirtualPlayerBattleRoyaleSabers)Sabers).ApplySettings(royaleSettings);
                }
            }
        }

        #endregion

        #region Setup

        [Inject] private readonly VirtualPlayerAvatarBody.Pool _avatarsPool = null!;
        [Inject] private readonly VirtualPlayerBattleRoyaleSabers.Pool _sabersPool = null!;
        [Inject] private readonly VirtualPlayerGameSabers _gameSabers = null!;
        [Inject] private readonly ReplayLaunchData _replayLaunchData = null!;
        [Inject] private readonly IVirtualPlayersManager _playersManager = null!;

        private BodySettings BodySettings => _replayLaunchData.Settings.BodySettings;

        private void Awake() {
            BodySettings.ConfigUpdatedEvent += HandleBodyConfigUpdated;
        }

        private void OnDestroy() {
            BodySettings.ConfigUpdatedEvent -= HandleBodyConfigUpdated;
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

        private readonly List<VirtualPlayerBodyAdapter> _spawnedAdapters = new();

        public IVirtualPlayerBody SpawnBody(IVirtualPlayer player) {
            var primary = player == _playersManager.PrimaryPlayer;

            var sabers = SpawnOrBorrowSabers(player, primary);
            var avatar = _avatarsPool.Spawn(player);

            var adapter = new VirtualPlayerBodyAdapter(avatar, sabers, primary);
            _spawnedAdapters.Add(adapter);

            var config = primary ?
                BodySettings.RequireConfig<BasicBodySettings>() :
                BodySettings.RequireConfig<BattleRoyaleBodySettings>();

            adapter.ApplySettings(config);

            return adapter;
        }

        public void DespawnBody(IVirtualPlayerBody body) {
            if (body is not VirtualPlayerBodyAdapter castedBody) {
                throw new InvalidOperationException("Unable to despawn a body which does not belong to the pool");
            }

            _avatarsPool.Despawn(castedBody.Avatar);
            ReleaseSabers(castedBody.Sabers, castedBody.UsesPrimarySabers);

            _spawnedAdapters.Remove(castedBody);
        }

        #endregion

        #region Config

        private void HandleBodyConfigUpdated(BodySettings _, object config) {
            if (config is not BasicBodySettings settings) {
                return;
            }

            foreach (var avatar in _spawnedAdapters) {
                avatar.ApplySettings(settings);
            }
        }

        #endregion
    }
}