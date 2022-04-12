using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.Models;
using UnityEngine;
using Zenject;

namespace BeatLeader.Replays
{
    public class ReplaySystemHelper
    {
        [Inject] private GameScenesManager _gameScenesManager;
        private protected StandardLevelScenesTransitionSetupDataSO _scenesTransitionSetupDataSO;
        private protected ReplayPlayer _player;
        private protected Replay _replay;

        public StandardLevelScenesTransitionSetupDataSO scenesTransitionSetupDataSO => _scenesTransitionSetupDataSO;
        public ReplayPlayer player => _player;
        public Replay replay => _replay;

        public void InstallRecorder()
        {

        }
        public void StartInReplayMode(StandardLevelScenesTransitionSetupDataSO transitionData, Replay replay)
        {
            _gameScenesManager.transitionDidFinishEvent += CreateFakePlayer;
            _scenesTransitionSetupDataSO = transitionData;
            _replay = replay;
            _gameScenesManager.PushScenes(_scenesTransitionSetupDataSO);
        }
        public void StopAndDisposeReplayStuff()
        {
            _scenesTransitionSetupDataSO = null;
            _replay = null;
            GameObject.Destroy(_player);
        }
        private void CreateFakePlayer(ScenesTransitionSetupDataSO data, DiContainer container)
        {
            _player = new GameObject("ReplayPlayer").AddComponent<ReplayPlayer>();
            _player.Init(_replay, ReplaySabersHelper.CreateFakeReplaySaber(SaberType.SaberA), ReplaySabersHelper.CreateFakeReplaySaber(SaberType.SaberB));
            _gameScenesManager.transitionDidFinishEvent -= CreateFakePlayer;
        }
    }
}
