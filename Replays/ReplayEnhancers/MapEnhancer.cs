using BeatLeader.Replays.Models;
using System.Collections.Generic;

namespace BeatLeader.Replays.ReplayEnhancers
{
    class MapEnhancer
    {
        public static IDifficultyBeatmap difficultyBeatmap;
        public static IPreviewBeatmapLevel previewBeatmapLevel;
        public static GameplayModifiers gameplayModifiers;
        public static PlayerSpecificSettings playerSpecificSettings;
        public static PracticeSettings practiceSettings;
        public static bool useTestNoteCutSoundEffects;
        public static EnvironmentInfoSO environmentInfo;
        public static ColorScheme colorScheme;
        public static float energy;

        public static void Enhance(Replay replay)
        {
            var info = replay.info;
            info.hash = previewBeatmapLevel.levelID.Replace("custom_level_", "");
            info.songName = difficultyBeatmap.level.songName;
            info.mapper = difficultyBeatmap.level.levelAuthorName;
            info.difficulty = difficultyBeatmap.difficulty.ToString();

            info.mode = difficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            info.environment = environmentInfo.environmentName;
            info.modifiers = string.Join(",", modifiers());
            info.leftHanded = playerSpecificSettings.leftHanded;
            info.height = playerSpecificSettings.automaticPlayerHeight ? 0 : playerSpecificSettings.playerHeight;

            if (practiceSettings != null)
            {
                info.startTime = practiceSettings.startSongTime;
                info.speed = practiceSettings.songSpeedMul;
            }
        }

        private static List<string> modifiers()
        {
            List<string> result = new();

            if (gameplayModifiers.disappearingArrows) { result.Add("DA"); }
            if (gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Faster) { result.Add("FS"); }
            if (gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower) { result.Add("SS"); }
            if (gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.SuperFast) { result.Add("SF"); }
            if (gameplayModifiers.ghostNotes) { result.Add("GN"); }
            if (gameplayModifiers.noArrows) { result.Add("NA"); }
            if (gameplayModifiers.noBombs) { result.Add("NB"); }
            if (gameplayModifiers.noFailOn0Energy && energy == 0) { result.Add("NF"); }
            if (gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles) { result.Add("NO"); }
            if (gameplayModifiers.strictAngles) { result.Add("SA"); }
            if (gameplayModifiers.proMode) { result.Add("PM"); }
            if (gameplayModifiers.smallCubes) { result.Add("SC"); }
            if (gameplayModifiers.failOnSaberClash) { result.Add("CS"); }
            if (gameplayModifiers.instaFail) { result.Add("IF"); }
            if (gameplayModifiers.energyType == GameplayModifiers.EnergyType.Battery) { result.Add("BE"); }

            return result;
        }
    }
}
