using System;
using System.Reflection;
using IPA.Loader;

namespace BeatLeader.Interop {
    internal static class SongCoreInterop {
        #region TryRefreshSongs

        public static bool TryRefreshSongs(bool fullRefresh) {
            try {
                var songCoreAssembly = PluginManager.GetPluginFromId("SongCore")!.Assembly;
                var songLoaderType = songCoreAssembly!.GetType("SongCore.Loader");
                var loaderInstanceFieldInfo = songLoaderType!.GetField("Instance", BindingFlags.Static | BindingFlags.Public);
                var refreshSongsMethodInfo = songLoaderType!.GetMethod("RefreshSongs", BindingFlags.Instance | BindingFlags.Public);

                var loaderInstance = loaderInstanceFieldInfo!.GetValue(null);
                refreshSongsMethodInfo!.Invoke(loaderInstance, new object[] {fullRefresh});
                return true;
            } catch (Exception e) {
                Plugin.Log.Debug($"RefreshSongs failed: {e}");
                return false;
            }
        }

        #endregion
    }
}