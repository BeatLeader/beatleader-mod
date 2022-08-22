using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeatLeader.Utils;
using Oculus.Platform;
using Steamworks;
using UnityEngine;
using UnityEngine.Networking;

namespace BeatLeader.API {
    internal static class Authentication {
        #region AuthPlatform

        public static AuthPlatform Platform { get; private set; }

        public static void SetPlatform(AuthPlatform platform) {
            Platform = platform;
        }

        public enum AuthPlatform {
            Undefined,
            Steam,
            OculusPC
        }

        #endregion

        #region Ticket

        public static Task<string> PlatformTicket() {
            return Platform switch {
                AuthPlatform.Steam => SteamTicket(),
                AuthPlatform.OculusPC => OculusTicket(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public static async Task<string> SteamTicket() {
            if (!SteamManager.Initialized) {
                Plugin.Log.Error("SteamManager is not initialized!");
                return null;
            }

            var getAuthResponseTcs = new TaskCompletionSource<bool>();
            var getAuthSessionTicketCallback = new Callback<GetAuthSessionTicketResponse_t>(resp => getAuthResponseTcs.TrySetResult(resp.m_eResult == EResult.k_EResultOK));

            string ticket = null;

            const int ticketBufferSize = 1024;
            var byteBuffer = new byte[ticketBufferSize];
            if (SteamUser.GetAuthSessionTicket(byteBuffer, ticketBufferSize, out var ticketLength) != HAuthTicket.Invalid) {
                ticket = BitConverter.ToString(byteBuffer, 0, (int)ticketLength).Replace("-", "");
            }

            await Task.WhenAny(getAuthResponseTcs.Task, Task.Delay(TimeSpan.FromSeconds(10.0))).ConfigureAwait(false);

            getAuthSessionTicketCallback.Dispose();
            return !getAuthResponseTcs.Task.IsCompleted || !getAuthResponseTcs.Task.Result ? null : ticket;
        }

        public static async Task<string> OculusTicket() {
            TaskCompletionSource<string> tcs = new();
            Users.GetAccessToken().OnComplete(delegate(Message<string> message) { tcs.TrySetResult(message.IsError ? null : message.Data); });
            return await tcs.Task;
        }

        #endregion

        #region Login

        private static bool _locked;
        private static bool _signedIn;

        public static void ResetLogin() {
            UnityWebRequest.ClearCookieCache(new Uri(BLConstants.BEATLEADER_API_URL));
            _signedIn = false;
        }

        public static IEnumerator EnsureLoggedIn(Action onSuccess, Action<string> onFail) {
            while (true) {
                if (!_locked) {
                    _locked = true;
                    break;
                }

                yield return new WaitUntil(() => !_locked);
            }

            try {
                if (_signedIn) {
                    onSuccess();
                    yield break;
                }

                yield return DoLogin(() => {
                    _signedIn = true;
                    onSuccess();
                }, onFail);
            } finally {
                _locked = false;
            }
        }

        private static IEnumerator DoLogin(Action onSuccess, Action<string> onFail) {
            if (!TryGetPlatformProvider(Platform, out var provider)) {
                Plugin.Log.Debug("Login failed! Unknown platform");
                onFail("Unknown platform");
                yield break;
            }
            
            var ticketTask = PlatformTicket();
            yield return new WaitUntil(() => ticketTask.IsCompleted);

            var authToken = ticketTask.Result;
            if (authToken == null) {
                Plugin.Log.Debug("Login failed! No auth token");
                onFail("No auth token");
                yield break;
            }

            var form = new List<IMultipartFormSection> {
                new MultipartFormDataSection("provider", provider),
                new MultipartFormDataSection("returnUrl", "/")
            };
            var request = UnityWebRequest.Post(string.Format(BLConstants.SIGNIN_WITH_TICKET, authToken), form);
            yield return request.SendWebRequest();

            switch (request.responseCode) {
                case 200:
                    Plugin.Log.Info("Login successful!");
                    onSuccess();
                    break;
                case BLConstants.MaintenanceStatus:
                    Plugin.Log.Debug("Login failed! Maintenance");
                    onFail("Maintenance");
                    break;
                default:
                    Plugin.Log.Debug($"Login failed! status: {request.responseCode} error: {request.error}");
                    onFail($"NetworkError: {request.responseCode}");
                    break;
            }
        }

        private static bool TryGetPlatformProvider(AuthPlatform platform, out string provider) {
            switch (platform) {
                case AuthPlatform.Steam:
                    provider = "steamTicket";
                    return true;
                case AuthPlatform.OculusPC:
                    provider = "oculusTicket";
                    return true;
                case AuthPlatform.Undefined:
                default:
                    provider = null;
                    return false;
            }
        }

        #endregion
    }
}