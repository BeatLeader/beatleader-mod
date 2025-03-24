using BeatLeader.Models;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayersManager : MonoBehaviour, IVirtualPlayersManager {
        #region Injection

        [Inject] private readonly IVirtualPlayerBodySpawner _bodySpawner = null!;
        [Inject] private readonly PlayerTransforms _playerTransforms = null!;
        [Inject] private readonly VirtualPlayer.Pool _virtualPlayersPool = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        #endregion

        #region VirtualPlayersManager

        public IReadOnlyList<IVirtualPlayer> Players => _virtualPlayers;
        public IVirtualPlayer PrimaryPlayer { get; private set; } = null!;

        public event Action<IVirtualPlayer>? PrimaryPlayerWasChangedEvent;

        public void SetPrimaryPlayer(IVirtualPlayer player) {
            if (!_virtualPlayers.Contains(player)) {
                return;
            }

            var newPlayer = (VirtualPlayer)player;
            var prevPlayer = (VirtualPlayer)PrimaryPlayer;

            _bodySpawner.DespawnBody(newPlayer.Body);
            _bodySpawner.DespawnBody(prevPlayer.Body);

            PrimaryPlayer = newPlayer;

            newPlayer.LateInit(_bodySpawner.SpawnBody(newPlayer));
            prevPlayer.LateInit(_bodySpawner.SpawnBody(prevPlayer));

            PrimaryPlayerWasChangedEvent?.Invoke(player);
        }

        #endregion

        #region Setup

        private void Awake() {
            LoadDummyControllers();

            var first = true;
            foreach (var replay in _launchData.Replays) {
                Spawn(replay, first);

                first = false;
            }
        }

        //TODO: set transforms of the primary player
        private void LoadDummyControllers() {
            _playerTransforms.SetField("_headTransform", CreateDummyController());
            _playerTransforms.SetField("_leftHandTransform", CreateDummyController());
            _playerTransforms.SetField("_rightHandTransform", CreateDummyController());
        }

        private static Transform CreateDummyController() {
            var go = new GameObject("DummyController");
            go.AddComponent<VRController>().enabled = false;
            return go.transform;
        }

        #endregion

        #region Spawn & Despawn

        private readonly List<IVirtualPlayer> _virtualPlayers = new();

        private void Spawn(IReplay replay, bool setPrimary) {
            var virtualPlayer = _virtualPlayersPool.Spawn();
            virtualPlayer.Init(replay);

            if (setPrimary) {
                PrimaryPlayer = virtualPlayer;
            }

            var body = _bodySpawner.SpawnBody(virtualPlayer);
            virtualPlayer.LateInit(body);

            _virtualPlayers.Add(virtualPlayer);
        }

        private void Despawn(VirtualPlayer player) {
            _bodySpawner.DespawnBody(player.Body);
            _virtualPlayersPool.Despawn(player);
            _virtualPlayers.Remove(player);
        }

        #endregion
    }
}