using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayersManager : MonoBehaviour, IVirtualPlayersManager {
        [Inject] private readonly VRControllersInstantiator _controllersInstantiator = null!;
        [Inject] private readonly VirtualPlayer.Pool _virtualPlayersPool = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public IReadOnlyList<VirtualPlayer> Players => _virtualPlayers;
        public VirtualPlayer PriorityPlayer => _priorityPlayer ?? _virtualPlayers.FirstOrDefault();

        public event Action<VirtualPlayer>? PriorityPlayerWasChangedEvent;

        private readonly List<VirtualPlayer> _virtualPlayers = new();
        private VirtualPlayer? _priorityPlayer;

        private void Awake() {
            foreach (var replayPair in _launchData.Replays) {
                Spawn(replayPair.Key, replayPair.Value);
            }
        }

        public void Spawn(Player player, Replay replay) {
            var virtualPlayer = _virtualPlayersPool.Spawn();
            player ??= new() { name = "Undefined Player" };
            var controllers = _controllersInstantiator.CreateInstance(
                player.name.Replace(" ", "") + "Controllers");
            virtualPlayer.Init(player, replay, controllers);
            _virtualPlayers.Add(virtualPlayer);
        }

        public void Despawn(VirtualPlayer player) {
            _virtualPlayersPool.Despawn(player);
            _virtualPlayers.Remove(player);
        }

        public void SetPriorityPlayer(VirtualPlayer player) {
            if (!_virtualPlayers.Contains(player)) return;
            _priorityPlayer = player;
        }
    }
}
