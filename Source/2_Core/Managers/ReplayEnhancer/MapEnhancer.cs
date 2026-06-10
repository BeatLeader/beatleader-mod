using BeatLeader.Models;
using System.Collections.Generic;
using BeatLeader.Models.Replay;

namespace BeatLeader.Core.Managers.ReplayEnhancer
{
    class MapEnhancer
    {
        public static BeatmapKey beatmapKey;
        public static BeatmapLevel beatmapLevel;
        public static PlayerSpecificSettings playerSpecificSettings;
        public static PracticeSettings practiceSettings;
        public static bool useTestNoteCutSoundEffects;
        public static string environmentName;
        public static ColorScheme colorScheme;

        public static void Enhance(Replay replay)
        {
            var info = replay.info;
            info.hash = beatmapKey.levelId.Replace("custom_level_", "");
            info.songName = beatmapLevel.songName;
            info.mapper = string.Join(",", beatmapLevel.allMappers);
            info.difficulty = beatmapKey.difficulty.ToString();

            info.mode = beatmapKey.beatmapCharacteristic.serializedName;
            info.environment = environmentName;
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
