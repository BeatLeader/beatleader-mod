using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Replays.ReplayEnhancers;
using Zenject;

namespace BeatLeader.Replays.Models
{
    public class ReplayDataProvider
    {
        public readonly PlayerTransforms playerTransforms;
        public readonly BeatmapObjectManager beatmapObjectManager;
        public readonly BeatmapObjectSpawnController beatmapObjectSpawnController;
        public readonly StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionData;
        public readonly PauseController pauseController;
        public readonly AudioTimeSyncController timeSyncController;
        public readonly ScoreController scoreController;
        public readonly PlayerHeadAndObstacleInteraction playerHeadAndObstacleInteraction;
        public readonly GameEnergyCounter gameEnergyCounter;
        public readonly PlayerHeightDetector playerHeightDetector;
        public readonly TrackingDeviceEnhancer trackingDeviceEnhancer;

        [Inject] public ReplayDataProvider(PlayerTransforms playerTransforms, BeatmapObjectManager beatmapObjectManager, BeatmapObjectSpawnController beatmapObjectSpawnController, StandardLevelScenesTransitionSetupDataSO transitionSetupDataSO, PauseController pauseController, AudioTimeSyncController audioTimeSyncController, ScoreController scoreController, PlayerHeadAndObstacleInteraction headObstacleInteraction, GameEnergyCounter gameEnergyCounter, PlayerHeightDetector heightDetector, TrackingDeviceEnhancer deviceEnhancer)
        {
            this.playerTransforms = playerTransforms;
            this.beatmapObjectManager = beatmapObjectManager;
            this.beatmapObjectSpawnController = beatmapObjectSpawnController;
            this.standardLevelScenesTransitionData = transitionSetupDataSO;
            this.pauseController = pauseController;
            this.timeSyncController = audioTimeSyncController;
            this.scoreController = scoreController;
            this.playerHeadAndObstacleInteraction = headObstacleInteraction;
            this.gameEnergyCounter = gameEnergyCounter;
            this.playerHeightDetector = heightDetector;
            this.trackingDeviceEnhancer = deviceEnhancer;
        }
    }
}
