using BeatLeader.Models;
using System.Collections.Generic;
using BeatLeader.Models.Replay;

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
            info.modifiers = string.Join(",", ModifiersManager.Modifiers.Where(m => ModifiersManager.GetModifierState(m.Id)).Select(m => m.Id));
            info.leftHanded = playerSpecificSettings.leftHanded;
            info.height = playerSpecificSettings.automaticPlayerHeight ? 0 : playerSpecificSettings.playerHeight;

            if (practiceSettings != null)
            {
                info.startTime = practiceSettings.startSongTime;
                info.speed = practiceSettings.songSpeedMul;
            }
        }
    }
}
