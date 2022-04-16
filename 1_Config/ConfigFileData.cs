using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using JetBrains.Annotations;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace BeatLeader {
    [UsedImplicitly]
    public class ConfigFileData {
        public static ConfigFileData Instance { get; set; }

        #region ConfigVersion

        public string ConfigVersion = ConfigDefaults.ConfigVersion;

        #endregion

        #region Enabled

        public bool Enabled = ConfigDefaults.Enabled;

        #endregion
    }
}