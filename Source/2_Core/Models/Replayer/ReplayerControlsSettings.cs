using JetBrains.Annotations;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerControlsSettings {
        /// <summary>
        /// Whether seeking/speed control via the VR controller thumbsticks is enabled.
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Whether seeking/speed control via FPFC keyboard hotkeys is enabled.
        /// </summary>
        public bool FpfcControlsEnabled { get; set; } = true;

        /// <summary>
        /// When <c>true</c>, the right stick (left/right) seeks the replay and the left stick (up/down)
        /// changes the playback speed. When <c>false</c>, the hands are swapped.
        /// </summary>
        public bool SeekOnRightHand { get; set; } = true;

        /// <summary>
        /// Base seek increment, in seconds, applied at 1x playback speed. The actual increment is scaled
        /// by the current playback speed, so slower playback seeks in smaller steps and faster in bigger ones.
        /// </summary>
        public float SeekStepSeconds { get; set; } = 5f;

        /// <summary>
        /// Playback speed multiplier increment applied per speed action (e.g. 0.1 == 10%).
        /// </summary>
        public float SpeedStep { get; set; } = 0.1f;
    }
}
