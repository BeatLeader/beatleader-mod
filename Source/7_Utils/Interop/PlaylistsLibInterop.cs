using BeatLeader.Utils;
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

        #region TryRefreshSongs

        public static BeatmapLevelPack? TryFindPlaylist(string filename) {
            try {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "BeatSaberPlaylistsLib");
                var playlistManagerType = assembly!.GetType("BeatSaberPlaylistsLib.PlaylistManager");
                
                var defaultManagerField = playlistManagerType.GetProperty("DefaultManager", BindingFlags.Static | BindingFlags.Public);
                
                var getAllMethodInfo = playlistManagerType.GetMethod("GetAllPlaylists", 
                    ReflectionUtils.DefaultFlags, new Type[] { });

                var manager = defaultManagerField!.GetValue(null);
                var playlists = (object[])getAllMethodInfo!.Invoke(manager, new object[] {});

                var playlistType = assembly!.GetType("BeatSaberPlaylistsLib.Types.Playlist");
                var filenameField = playlistType.GetProperty("Filename", BindingFlags.Instance | BindingFlags.Public);

                var playlistWrapper = playlists.FirstOrDefault(p => (string)filenameField!.GetValue(p) == filename);
                if (playlistWrapper == null) {
                    return null;
                }

                var levelPackField = playlistType.GetProperty("PlaylistLevelPack", BindingFlags.Instance | BindingFlags.Public);

                return (BeatmapLevelPack)levelPackField.GetValue(playlistWrapper);
            } catch (Exception e) {
                Plugin.Log.Critical($"RefreshPlaylists failed: {e}");
                return null;
            }
        }

        #endregion
    }
}