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
        public IVirtualPlayer PrimaryPlayer => _primaryPlayer!;

        public event Action<IVirtualPlayer>? PrimaryPlayerWasChangedEvent;

        private IVirtualPlayer? _primaryPlayer;

        public void SetPrimaryPlayer(IVirtualPlayer player) {
            if (!_virtualPlayers.Contains(player)) return;

            var previousPrimaryPlayer = _primaryPlayer;
            _primaryPlayer = player;
            //refreshing previous first to release base-game sabers in case they are used
            previousPrimaryPlayer?.Body.RefreshVisuals();
            _primaryPlayer.Body.RefreshVisuals();
            
            PrimaryPlayerWasChangedEvent?.Invoke(player);
        }

        #endregion

        #region Setup

        private void Awake() {
            LoadDummyControllers();
            foreach (var replay in _launchData.Replays) Spawn(replay);
            if (_virtualPlayers.Count is not 0) SetPrimaryPlayer(_virtualPlayers[0]);
        }

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

        private void Spawn(IReplay replay) {
            var virtualPlayer = _virtualPlayersPool.Spawn();
            virtualPlayer.Init(replay);

            var body = _bodySpawner.SpawnBody(this, virtualPlayer);
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