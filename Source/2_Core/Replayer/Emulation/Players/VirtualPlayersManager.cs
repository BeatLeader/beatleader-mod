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
        [Inject] private readonly IVirtualPlayerSabersSpawner _sabersSpawner = null!;
        [Inject] private readonly PlayerTransforms _playerTransforms = null!;
        [Inject] private readonly VirtualPlayer.Pool _virtualPlayersPool = null!;
        [Inject] private readonly ReplayLaunchData _launchData = null!;

        #endregion

        #region VirtualPlayersManager

        public VirtualPlayerConfig PrimaryPlayerConfig { get; private set; } = null!;
        public VirtualPlayerConfig PlayerConfig { get; private set; } = null!;

        public IReadOnlyList<IVirtualPlayer> Players => _virtualPlayers;
        public IVirtualPlayer PrimaryPlayer => _primaryPlayer!;

        public event Action<IVirtualPlayer>? PrimaryPlayerWasChangedEvent;
        
        private IVirtualPlayer? _primaryPlayer;

        public void SetPrimaryPlayer(IVirtualPlayer player) {
            if (!_virtualPlayers.Contains(player)) return;

            if (_primaryPlayer is not null) ReloadPlayerConfigs(PrimaryPlayer, false);
            var primaryBody = ReloadPlayerConfigs(player, true);

            SetPrimaryControllers(primaryBody.ControllersProvider);
            _primaryPlayer = player;
            PrimaryPlayerWasChangedEvent?.Invoke(player);
        }

        #endregion

        #region Setup

        private void Awake() {
            LoadConfigs();
            foreach (var replay in _launchData.Replays) Spawn(replay);
            if (_virtualPlayers.Count is not 0) SetPrimaryPlayer(_virtualPlayers[0]);
        }

        private void LoadConfigs() {
            PrimaryPlayerConfig = CreateDefaultConfig();
            PrimaryPlayerConfig.SabersConfig.SetPrimary(true);
            PlayerConfig = CreateDefaultConfig();
        }

        private VirtualPlayerConfig CreateDefaultConfig() {
            var bodyConfig = new VirtualPlayerBodyConfig(_bodySpawner.BodyModel);
            var sabersConfig = new VirtualPlayerSabersConfig();
            return new(bodyConfig, sabersConfig);
        }

        #endregion

        #region Spawn & Despawn

        private readonly List<IVirtualPlayer> _virtualPlayers = new();

        private void Spawn(IReplay replay) {
            var virtualPlayer = _virtualPlayersPool.Spawn();
            virtualPlayer.Init(replay);
            
            var body = _bodySpawner.SpawnBody(virtualPlayer);
            var sabers = _sabersSpawner.SpawnSabers(virtualPlayer);
            virtualPlayer.InitBodyAndSabers(body, sabers);
            
            ReloadPlayerConfigs(virtualPlayer, false);
            _virtualPlayers.Add(virtualPlayer);
        }

        private void Despawn(VirtualPlayer player) {
            _bodySpawner.DespawnBody(player.Body);
            _virtualPlayersPool.Despawn(player);
            _virtualPlayers.Remove(player);
        }

        #endregion

        #region Tools

        private IVirtualPlayerBody ReloadPlayerConfigs(IVirtualPlayer targetPlayer, bool primary) {
            var player = (VirtualPlayer)targetPlayer;
            var config = primary ? PrimaryPlayerConfig : PlayerConfig;
            player.Body.ApplyConfig(config.BodyConfig);
            player.Sabers.ApplyConfig(config.SabersConfig);
            return player.Body;
        }

        private void SetPrimaryControllers(IVRControllersProvider provider) {
            _playerTransforms.SetField("_headTransform", provider.Head.transform);
            _playerTransforms.SetField("_leftHandTransform", provider.LeftHand.transform);
            _playerTransforms.SetField("_rightHandTransform", provider.RightHand.transform);
        }

        #endregion
    }
}