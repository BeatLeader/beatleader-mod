using System;
using System.Collections;
using BeatLeader.API;
using BeatLeader.Manager;
using BeatLeader.Utils;
using UnityEngine;

namespace BeatLeader.DataManager {
    internal class OculusMigrationManager : MonoBehaviour {
        #region IsMigrationRequired

        public static event Action<bool> IsMigrationRequiredChangedEvent;

        private static bool _isMigrationRequired;

        public static bool IsMigrationRequired {
            get => _isMigrationRequired;
            private set {
                if (_isMigrationRequired.Equals(value)) return;
                _isMigrationRequired = value;
                IsMigrationRequiredChangedEvent?.Invoke(value);
            }
        }

        #endregion

        #region Start

        private void Start() {
            LeaderboardEvents.OculusMigrationButtonWasPressedAction += OnOculusMigrationButtonWasPressed;
            GetOculusUserRequest.StateChangedEvent += GetOculusUserRequest_StateChangedEvent;

            CheckMigrationState();
        }

        private void GetOculusUserRequest_StateChangedEvent(WebRequests.IWebRequest<Models.OculusUserInfo> instance, WebRequests.RequestState state, string? failReason) {
            if (state == WebRequests.RequestState.Finished) {
                IsMigrationRequired = !instance.Result.migrated;
            } else if (state == WebRequests.RequestState.Failed) {
                Plugin.Log.Debug($"CheckMigrationState failed: {failReason}");
            }
        }

        private void OnDestroy() {
            LeaderboardEvents.OculusMigrationButtonWasPressedAction -= OnOculusMigrationButtonWasPressed;
        }

        #endregion

        #region CheckMigrationState

        private void CheckMigrationState() {
            GetOculusUserRequest.Send().RunCatching();
        }

        #endregion

        #region OnOculusMigrationButtonWasPressed

        private Coroutine _signInTask;

        private void OnOculusMigrationButtonWasPressed() {
            if (_signInTask != null) {
                StopCoroutine(_signInTask);
            }

            _signInTask = StartCoroutine(OpenSigninPageCoroutine());
        }

        private static IEnumerator OpenSigninPageCoroutine() {
            var ticketTask = Authentication.PlatformTicket();
            yield return new WaitUntil(() => ticketTask.IsCompleted);

            var authToken = ticketTask.Result;
            if (authToken == null) {
                LeaderboardEvents.ShowStatusMessage("User authentication failed!", LeaderboardEvents.StatusMessageType.Bad);
                yield break; // auth failed
            }

            EnvironmentUtils.OpenBrowserPage(string.Format(BLConstants.OCULUS_PC_SIGNIN, authToken));
            IsMigrationRequired = false;
        }

        #endregion
    }
}