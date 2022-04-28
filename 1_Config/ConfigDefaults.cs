using BeatLeader.Models;

namespace BeatLeader {
    internal static class ConfigDefaults {
        #region ConfigVersion

        public const string ConfigVersion = "1.0";

        #endregion

        #region Enabled

        public const bool Enabled = true;

        #endregion

        #region ScoresContext

        public static readonly ScoresContext ScoresContext = ScoresContext.Modifiers;

        #endregion
    }
}