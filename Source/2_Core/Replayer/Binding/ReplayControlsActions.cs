using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.Replayer.Binding {
    internal static class ReplayControlsActions {
        private const float MinSpeedMultiplier = 0.1f;
        private const float MaxSpeedMultiplier = 2f;

        public static bool FpfcHotkeysEnabled(ReplayerControlsSettings settings) {
            return !InputUtils.UsesFPFC || settings.FpfcControlsEnabled;
        }

        public static void Seek(
            IBeatmapTimeController timeController,
            ReplayerControlsSettings settings,
            int direction
        ) {
            var speed = Mathf.Max(timeController.SongSpeedMultiplier, 0.01f);
            var delta = settings.SeekStepSeconds * speed * direction;
            timeController.Rewind(timeController.SongTime + delta);
        }

        public static void ChangeSpeed(
            IBeatmapTimeController timeController,
            ReplayerControlsSettings settings,
            int direction,
            bool precise = false
        ) {
            var newSpeed = timeController.SongSpeedMultiplier + (settings.SpeedStep / (precise ? 10.0f : 1.0f)) * direction;
            newSpeed = Mathf.Clamp(newSpeed, MinSpeedMultiplier, MaxSpeedMultiplier);
            timeController.SetSpeedMultiplier(Mathf.Round(newSpeed * 100f) / 100f);
        }
    }
}
