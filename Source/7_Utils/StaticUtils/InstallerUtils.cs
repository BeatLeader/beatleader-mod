using System.Reflection;
using Zenject;

namespace BeatLeader {
    public static class InstallerUtils {
        #region GetContainer

        private static readonly PropertyInfo ContainerPropertyInfo = typeof(MonoInstallerBase).GetProperty(
            "Container",
            BindingFlags.Instance | BindingFlags.NonPublic
        );

        public static DiContainer GetContainer(this MonoInstallerBase monoInstallerBase) {
            return (DiContainer)ContainerPropertyInfo.GetValue(monoInstallerBase);
        }

        #endregion
    }
}