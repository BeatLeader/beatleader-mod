using System;
using BeatLeader.API.Methods;
using BeatLeader.Models;
using JetBrains.Annotations;
using Zenject;

namespace BeatLeader.DataManager {
    [UsedImplicitly]
    internal class ModVersionChecker : IInitializable, IDisposable {
        #region CurrentReleaseInfo

        public static ReleaseInfo CurrentReleaseInfo = new() {
            version = Plugin.Version.ToString(),
            link = null
        };

        #endregion

        #region LatestReleaseInfo

        public static event Action<ReleaseInfo> LatestVersionChangedEvent;

        private static ReleaseInfo _latestReleaseInfo = CurrentReleaseInfo;

        public static ReleaseInfo LatestReleaseInfo {
            get => _latestReleaseInfo;
            private set {
                if (_latestReleaseInfo.Equals(value)) return;
                _latestReleaseInfo = value;
                var current = Version.TryParse(CurrentReleaseInfo.version, out var tmp) ? tmp : new Version(0, 0);
                var latest = Version.TryParse(value.version, out tmp) ? tmp : new Version(0, 0);
                IsUpToDate = current >= latest;
                LatestVersionChangedEvent?.Invoke(value);
            }
        }

        #endregion

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

        public void Initialize() {
            LatestReleasesRequest.AddStateListener(OnLatestReleasesRequestStateChanged);
            LatestReleasesRequest.SendRequest();
        }

        public void Dispose() {
            LatestReleasesRequest.RemoveStateListener(OnLatestReleasesRequestStateChanged);
        }

        #endregion

        #region OnLatestReleasesRequestStateChanged

        private static void OnLatestReleasesRequestStateChanged(API.RequestState state, LatestReleases result, string failReason) {
            if (state is not API.RequestState.Finished) return;
            LatestReleaseInfo = result.pc;
        }

        #endregion
    }
}