using System;

namespace BeatLeader {
    public static class PluginConfig {
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
    }
}