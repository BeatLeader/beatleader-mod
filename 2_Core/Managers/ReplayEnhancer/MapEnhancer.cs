using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PlayerSaveData;

namespace BeatLeader.Core.Managers.ReplayEnhancer
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

        public static void Enhance(Replay replay)
        {
            var info = replay.info;
            info.hash = previewBeatmapLevel.levelID.Replace("custom_level_", "");
            info.songName = difficultyBeatmap.level.songName;
            info.mapper = difficultyBeatmap.level.levelAuthorName;
            info.difficulty = difficultyBeatmap.difficulty.ToString();

            info.mode = difficultyBeatmap.level.beatmapLevelData.difficultyBeatmapSets[0].beatmapCharacteristic.serializedName;
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
            if (gameplayModifiers.noFailOn0Energy) { result.Add("NF"); }
            if (gameplayModifiers.enabledObstacleType == GameplayModifiers.EnabledObstacleType.NoObstacles) { result.Add("NO"); }

            return result;
        }
    }
}
