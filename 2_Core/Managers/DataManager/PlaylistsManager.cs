using System;
using System.Collections.Generic;
using BeatLeader.Interop;
using BeatLeader.Manager;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class PlaylistsManager : MonoBehaviour {
        #region Playlists

        private static readonly Dictionary<PlaylistType, PlaylistInfo> Playlists = new() {
            { PlaylistType.Nominated, new PlaylistInfo(BLConstants.PlaylistLink("nominated"),"BeatLeader nominated") },
            { PlaylistType.Qualified, new PlaylistInfo(BLConstants.PlaylistLink("qualified"),"BeatLeader qualified") },
            { PlaylistType.Ranked, new PlaylistInfo(BLConstants.PlaylistLink("ranked"),"BeatLeader ranked") },
        };

        private static bool TryGetPlaylistInfo(PlaylistType playlistType, out PlaylistInfo playlistInfo) {
            if (!Playlists.ContainsKey(playlistType)) {
                playlistInfo = null;
                return false;
            }

            playlistInfo = Playlists[playlistType];
            return true;
        }

        private class PlaylistInfo {
            public readonly string Link;
            public readonly string FileName;
            public PlaylistState State = PlaylistState.NotFound;

            public PlaylistInfo(string link, string fileName) {
                Link = link;
                FileName = fileName;
            }
        }

        public enum PlaylistState {
            NotFound,
            Outdated,
            UpToDate
        }

        public enum PlaylistType {
            Nominated,
            Qualified,
            Ranked
        }

        #endregion

        #region PlaylistState

        public static event Action<PlaylistType, PlaylistState> PlaylistStateChangedEvent;

        private static void SetPlaylistState(PlaylistType playlistType, PlaylistState state) {
            if (!TryGetPlaylistInfo(playlistType, out var playlistInfo)) return;
            if (playlistInfo.State == state) return;
            playlistInfo.State = state;
            PlaylistStateChangedEvent?.Invoke(playlistType, state);
        }

        public static PlaylistState GetPlaylistState(PlaylistType playlistType) {
            return !TryGetPlaylistInfo(playlistType, out var playlistInfo) ? PlaylistState.NotFound : playlistInfo.State;
        }

        #endregion

        #region Start

        private void Start() {
            LeaderboardEvents.PlaylistUpdateButtonWasPressedAction += UpdatePlaylist;
            VerifyPlaylistVersion(PlaylistType.Nominated);
            VerifyPlaylistVersion(PlaylistType.Qualified);
            VerifyPlaylistVersion(PlaylistType.Ranked);
        }

        private void OnDestroy() {
            LeaderboardEvents.PlaylistUpdateButtonWasPressedAction -= UpdatePlaylist;
        }

        #endregion

        #region VerifyPlaylistVersion

        private void VerifyPlaylistVersion(PlaylistType playlistType) {
            if (!TryGetPlaylistInfo(playlistType, out var playlistInfo)) return;

            if (!FileManager.TryReadPlaylist(playlistInfo.FileName, out var stored)) {
                SetPlaylistState(playlistType, PlaylistState.NotFound);
                return;
            }

            StartCoroutine(
                HttpUtils.GetBytes(
                    playlistInfo.Link,
                    bytes => SetPlaylistState(playlistType, ComparePlaylists(bytes, stored) ? PlaylistState.UpToDate : PlaylistState.Outdated),
                    (reason) => Plugin.Log.Debug($"{playlistType} playlist check failed: {reason}")
                )
            );
        }

        private static bool ComparePlaylists(byte[] a, byte[] b) {
            if (a.Length != b.Length) return false;

            for (var i = 0; i < a.Length; i++) {
                if (a[i] != b[i]) return false;
            }

            return true;
        }

        #endregion

        #region UpdatePlaylist

        public static event Action<PlaylistType> PlaylistUpdateStartedEvent;
        public static event Action<PlaylistType> PlaylistUpdateFinishedEvent;

        public void UpdatePlaylist(PlaylistType playlistType) {
            if (!TryGetPlaylistInfo(playlistType, out var playlistInfo)) return;
            PlaylistUpdateStartedEvent?.Invoke(playlistType);

            StartCoroutine(
                HttpUtils.GetBytes(
                    playlistInfo.Link,
                    bytes => {
                        FileManager.DeletePlaylist(playlistInfo.FileName);
                        
                        if (FileManager.TrySaveRankedPlaylist(playlistInfo.FileName, bytes)) {
                            PlaylistsLibInterop.TryRefreshPlaylists(true);
                            SongCoreInterop.TryRefreshSongs(false);
                            SetPlaylistState(playlistType, PlaylistState.UpToDate);
                        }

                        PlaylistUpdateFinishedEvent?.Invoke(playlistType);
                    },
                    (reason) => {
                        Plugin.Log.Debug($"{playlistType} playlist update failed: {reason}");
                        PlaylistUpdateFinishedEvent?.Invoke(playlistType);
                    }
                )
            );
        }

        #endregion
    }
}