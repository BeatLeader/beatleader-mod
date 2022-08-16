using System;
using BeatLeader.Interop;
using BeatLeader.Manager;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class RankedPlaylistManager : MonoBehaviour {
        #region IsUpToDate

        public static event Action<bool> IsUpToDateChangedEvent;

        private static bool _isUpToDate = true;

        public static bool IsUpToDate {
            get => _isUpToDate;
            private set {
                if (_isUpToDate.Equals(value)) return;
                _isUpToDate = value;
                IsUpToDateChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region Start

        private void Start() {
            LeaderboardEvents.RankedPlaylistUpdateButtonWasPressedAction += UpdatePlaylist;
            CheckPlaylist();
        }

        private void OnDestroy() {
            LeaderboardEvents.RankedPlaylistUpdateButtonWasPressedAction -= UpdatePlaylist;
        }

        #endregion

        #region CheckPlaylist

        private Coroutine _checkTask;

        private void CheckPlaylist() {
            if (_checkTask != null) {
                StopCoroutine(_checkTask);
            }

            var hasStored = FileManager.TryReadRankedPlaylist(out var stored);

            if (!hasStored) {
                IsUpToDate = false;
                return;
            }

            _checkTask = StartCoroutine(
                HttpUtils.GetBytes(
                    BLConstants.RANKED_PLAYLIST,
                    bytes => IsUpToDate = ComparePlaylists(bytes, stored),
                    (reason) => Plugin.Log.Debug($"Playlist check failed: {reason}")
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

        public static event Action PlaylistUpdateStartedEvent;
        public static event Action PlaylistUpdateFinishedEvent;

        private Coroutine _updateTask;

        public void UpdatePlaylist() {
            if (_updateTask != null) {
                StopCoroutine(_updateTask);
            }

            PlaylistUpdateStartedEvent?.Invoke();

            _updateTask = StartCoroutine(
                HttpUtils.GetBytes(
                    BLConstants.RANKED_PLAYLIST,
                    bytes =>
                    {
                        if (FileManager.TrySaveRankedPlaylist(bytes)) {
                            PlaylistsLibInterop.TryRefreshPlaylists(true);
                            SongCoreInterop.TryRefreshSongs(false);
                            IsUpToDate = true;
                        }

                        PlaylistUpdateFinishedEvent?.Invoke();
                    },
                    (reason) =>
                    {
                        Plugin.Log.Debug($"Playlist update failed: {reason}");
                        PlaylistUpdateFinishedEvent?.Invoke();
                    }
                )
            );
        }

        #endregion
    }
}