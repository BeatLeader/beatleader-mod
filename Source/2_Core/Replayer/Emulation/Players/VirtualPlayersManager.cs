using BeatLeader.Models;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayersManager : MonoBehaviour, IVirtualPlayersManager {
        [Inject] private readonly IVRControllersManager _controllersManager = null!;
        [Inject] private readonly PlayerTransforms _playerTransforms = null!;
        [Inject] private readonly VirtualPlayer.Pool _virtualPlayersPool = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public IReadOnlyList<IVirtualPlayer> Players => _virtualPlayers;
        public IVirtualPlayer PriorityPlayer => _priorityPlayer!;

        public event Action<IVirtualPlayer>? PriorityPlayerWasChangedEvent;

        private readonly List<IVirtualPlayer> _virtualPlayers = new();
        private IVirtualPlayer? _priorityPlayer;

        private void Awake() {
            foreach (var replay in _launchData.Replays) Spawn(replay);
            if (_virtualPlayers.Count is not 0) SetPriorityPlayer(_virtualPlayers[0]);
        }

        private void Spawn(IReplay replay) {
            var virtualPlayer = _virtualPlayersPool.Spawn();
            var controllers = _controllersManager.SpawnControllersProvider(false);
            virtualPlayer.Init(replay);
            virtualPlayer.ApplyControllers(controllers);
            _virtualPlayers.Add(virtualPlayer);
        }

        private void Despawn(VirtualPlayer player) {
            _virtualPlayersPool.Despawn(player);
            _virtualPlayers.Remove(player);
        }

        public void SetPriorityPlayer(IVirtualPlayer player) {
            if (!_virtualPlayers.Contains(player)) return;

            if (_priorityPlayer is not null) ReloadControllers(PriorityPlayer, false);
            var primaryControllers = ReloadControllers(player, true)!;

            SetPriorityControllers(primaryControllers);
            _priorityPlayer = player;
            PriorityPlayerWasChangedEvent?.Invoke(player);
        }

        private IVRControllersProvider ReloadControllers(IVirtualPlayer targetPlayer, bool primary) {
            _controllersManager.DespawnControllersProvider(targetPlayer.ControllersProvider);
            var controllers = _controllersManager.SpawnControllersProvider(primary);

            ((VirtualPlayer)targetPlayer).ApplyControllers(controllers);
            return controllers;
        }

        private void SetPriorityControllers(IVRControllersProvider provider) {
            _playerTransforms.SetField("_headTransform", provider.Head.transform);
            _playerTransforms.SetField("_leftHandTransform", provider.LeftSaber.transform);
            _playerTransforms.SetField("_rightHandTransform", provider.RightSaber.transform);
        }
    }
}