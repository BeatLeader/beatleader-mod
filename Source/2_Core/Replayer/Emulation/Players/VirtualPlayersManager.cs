using BeatLeader.Models;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using BeatLeader.Models.AbstractReplay;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayersManager : MonoBehaviour, IVirtualPlayersManager {
        [Inject] private readonly VRControllersInstantiator _controllersInstantiator = null!;
        [Inject] private readonly PlayerTransforms _playerTransforms = null!;
        [Inject] private readonly VirtualPlayer.Pool _virtualPlayersPool = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public IReadOnlyList<IVirtualPlayer> Players => _virtualPlayers;
        public IVirtualPlayer PriorityPlayer { get; private set; } = null!;

        public event Action<IVirtualPlayer>? PriorityPlayerWasChangedEvent;

        private readonly List<IVirtualPlayer> _virtualPlayers = new();

        private void Awake() {
            foreach (var replay in _launchData.Replays) Spawn(replay);
            if (_virtualPlayers.Count is not 0) SetPriorityPlayer(_virtualPlayers[0]);
        }

        private void Spawn(IReplay replay) {
            var virtualPlayer = _virtualPlayersPool.Spawn();
            var controllers = _controllersInstantiator
                .CreateInstance(replay.ReplayData.Player?.Name + "Controllers");
            virtualPlayer.Init(replay, controllers);
            _virtualPlayers.Add(virtualPlayer);
        }

        private void Despawn(VirtualPlayer player) {
            _virtualPlayersPool.Despawn(player);
            _virtualPlayers.Remove(player);
        }

        public void SetPriorityPlayer(IVirtualPlayer player) {
            if (!_virtualPlayers.Contains(player)) return;
            SetPriorityControllers(player.ControllersProvider!);
            PriorityPlayer = player;
            PriorityPlayerWasChangedEvent?.Invoke(player);
        }

        private void SetPriorityControllers(IVRControllersProvider provider) {
            _playerTransforms.SetField("_headTransform", provider.Head.transform);
            _playerTransforms.SetField("_leftHandTransform", provider.LeftSaber.transform);
            _playerTransforms.SetField("_rightHandTransform", provider.RightSaber.transform);
        }
    }
}
