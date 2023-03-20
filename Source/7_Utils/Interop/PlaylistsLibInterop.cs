using System;
using System.Linq;
using System.Reflection;

namespace BeatLeader.Interop {
    internal static class PlaylistsLibInterop {
        #region TryRefreshSongs

        public static bool TryRefreshPlaylists(bool fullRefresh) {
            try {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BeatSaberPlaylistsLib");
                var playlistManagerType = assembly!.GetType("BeatSaberPlaylistsLib.PlaylistManager");
                var defaultManagerField = playlistManagerType.GetProperty("DefaultManager", BindingFlags.Static | BindingFlags.Public);
                var refreshMethodInfo = playlistManagerType.GetMethod("RefreshPlaylists", BindingFlags.Instance | BindingFlags.Public);

                var manager = defaultManagerField!.GetValue(null);
                refreshMethodInfo!.Invoke(manager, new object[] {fullRefresh});
                return true;
            } catch (Exception e) {
                Plugin.Log.Debug($"RefreshPlaylists failed: {e}");
                return false;
            }
        }

        #endregion
    }
}