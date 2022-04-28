using System;
using BeatLeader.Models;

namespace BeatLeader {
    internal static class PluginConfig {
        #region Enabled

        public static event Action<bool> OnEnabledChangedEvent;

        public static bool Enabled {
            get => ConfigFileData.Instance.Enabled;
            set {
                if (ConfigFileData.Instance.Enabled == value) return;
                ConfigFileData.Instance.Enabled = value;
                OnEnabledChangedEvent?.Invoke(value);
            }
        }

        #endregion
        
        #region ScoresContext

        public static event Action<ScoresContext> ScoresContextChangedEvent;

        public static ScoresContext ScoresContext {
            get => ConfigFileData.Instance.ScoresContext;
            set {
                if (ConfigFileData.Instance.ScoresContext.Equals(value)) return;
                ConfigFileData.Instance.ScoresContext = value;
                ScoresContextChangedEvent?.Invoke(value);
            }
        }

        #endregion
    }
}