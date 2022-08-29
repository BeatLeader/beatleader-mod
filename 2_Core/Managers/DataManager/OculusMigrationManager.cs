using System;
using System.Collections;
using BeatLeader.API;
using BeatLeader.API.Methods;
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
            CheckMigrationState();
        }

        private void OnDestroy() {
            LeaderboardEvents.OculusMigrationButtonWasPressedAction -= OnOculusMigrationButtonWasPressed;
        }

        #endregion

        #region CheckMigrationState

        private Coroutine _checkTask;

        private void CheckMigrationState() {
            if (_checkTask != null) {
                StopCoroutine(_checkTask);
            }

            _checkTask = StartCoroutine(
                GetOculusUserRequest.SendRequest(
                    userInfo => IsMigrationRequired = !userInfo.migrated,
                    (reason) => Plugin.Log.Debug($"CheckMigrationState failed: {reason}")
                )
            );
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
            var ticketTask = Authentication.OculusTicket();
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