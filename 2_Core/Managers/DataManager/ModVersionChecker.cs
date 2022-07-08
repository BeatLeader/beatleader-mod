using System;
using BeatLeader.Models;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class ModVersionChecker : MonoBehaviour {
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
                IsUpToDate = value.version.Equals(CurrentReleaseInfo.version);
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

        private void Start() {
            CheckLatestVersion();
        }

        #endregion

        #region CheckLatestVersion

        private Coroutine _checkVersionTask;

        private void CheckLatestVersion() {
            if (_checkVersionTask != null) {
                StopCoroutine(_checkVersionTask);
            }

            _checkVersionTask = StartCoroutine(
                HttpUtils.GetData<LatestReleases>(
                    BLConstants.LATEST_RELEASES,
                    (result) => LatestReleaseInfo = result.pc,
                    (reason) => Plugin.Log.Debug($"ModVersion check failed: {reason}")
                )
            );
        }

        #endregion
    }
}