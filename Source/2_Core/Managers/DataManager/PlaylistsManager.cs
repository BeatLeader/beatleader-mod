using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.API;
using BeatLeader.Interop;
using BeatLeader.Manager;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class PlaylistsManager : MonoBehaviour {
        #region Playlists

        private static readonly Dictionary<PlaylistType, PlaylistInfo> Playlists = new() {
            { PlaylistType.Nominated, new PlaylistInfo("nominated", "BeatLeader nominated") },
            { PlaylistType.Qualified, new PlaylistInfo("qualified", "BeatLeader qualified") },
            { PlaylistType.Ranked, new PlaylistInfo("ranked", "BeatLeader ranked") },
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
            public readonly string PlaylistId;
            public readonly string FileName;
            public PlaylistState State = PlaylistState.NotFound;

            public PlaylistInfo(string playlistId, string fileName) {
                PlaylistId = playlistId;
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

        private async Task VerifyPlaylistVersion(PlaylistType playlistType) {
            if (!TryGetPlaylistInfo(playlistType, out var playlistInfo)) return;

            if (!FileManager.TryReadPlaylist(playlistInfo.FileName, out var stored)) {
                SetPlaylistState(playlistType, PlaylistState.NotFound);
                return;
            }

            var result = await PlaylistRequest.Send(playlistInfo.PlaylistId).Join();
            if (result.RequestState == WebRequests.RequestState.Finished) {
                SetPlaylistState(playlistType, ComparePlaylists(result.Result, stored) ? PlaylistState.UpToDate : PlaylistState.Outdated);
            } else if (result.RequestState == WebRequests.RequestState.Failed) {
                Plugin.Log.Debug($"{playlistType} playlist check failed: {result.FailReason}");
            }
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

        public async Task UpdatePlaylistAsync(PlaylistType playlistType) {
            if (!TryGetPlaylistInfo(playlistType, out var playlistInfo)) return;
            PlaylistUpdateStartedEvent?.Invoke(playlistType);

            var result = await PlaylistRequest.Send(playlistInfo.PlaylistId).Join();

            if (result.RequestState == WebRequests.RequestState.Finished) {
                FileManager.DeletePlaylist(playlistInfo.FileName);

                if (FileManager.TrySaveRankedPlaylist(playlistInfo.FileName, result.Result)) {
                    PlaylistsLibInterop.TryRefreshPlaylists(true);
                    SongCore.Loader.Instance.RefreshSongs(false);
                    SetPlaylistState(playlistType, PlaylistState.UpToDate);
                }

                PlaylistUpdateFinishedEvent?.Invoke(playlistType);
            } else if (result.RequestState == WebRequests.RequestState.Failed) {
                Plugin.Log.Debug($"{playlistType} playlist update failed: {result.FailReason}");
                PlaylistUpdateFinishedEvent?.Invoke(playlistType);
            }
        }

        public void UpdatePlaylist(PlaylistType playlistType) {
            UpdatePlaylistAsync(playlistType).RunCatching();
        }

        #endregion
    }
}