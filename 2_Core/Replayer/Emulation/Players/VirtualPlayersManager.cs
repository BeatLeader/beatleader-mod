using BeatLeader.Models;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replayer.Emulation {
    internal class VirtualPlayersManager : MonoBehaviour, IVirtualPlayersManager {
        [Inject] private readonly VRControllersInstantiator _controllersInstantiator = null!;
        [Inject] private readonly PlayerTransforms _playerTransforms = null!;
        [Inject] private readonly VirtualPlayer.Pool _virtualPlayersPool = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        public IReadOnlyList<VirtualPlayer> Players => _virtualPlayers;
        public VirtualPlayer? PriorityPlayer { get; private set; }

        public event Action<VirtualPlayer>? PriorityPlayerWasChangedEvent;

        private readonly List<VirtualPlayer> _virtualPlayers = new();

        private void Awake() {
            foreach (var replayPair in _launchData.Replays) {
                Spawn(replayPair.Key, replayPair.Value);
            }
            if (_virtualPlayers.Count != 0) {
                SetPriorityPlayer(_virtualPlayers[0]);
            }
        }

        public void Spawn(Player player, Replay replay) {
            var virtualPlayer = _virtualPlayersPool.Spawn();
            player ??= new() { name = "Undefined Player" };
            var name = player.name.Replace(" ", "") + "Controllers";
            var controllers = _controllersInstantiator.CreateInstance(name);
            virtualPlayer.Init(player, replay, controllers);
            _virtualPlayers.Add(virtualPlayer);
        }

        public void Despawn(VirtualPlayer player) {
            _virtualPlayersPool.Despawn(player);
            _virtualPlayers.Remove(player);
        }

        public void SetPriorityPlayer(VirtualPlayer player) {
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
