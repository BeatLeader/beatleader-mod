using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using BS_Utils.Gameplay;
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

        public static async Task<string> PlatformTicket() {
            await GetUserInfo.GetUserAsync();
            var platformUserModel = Resources.FindObjectsOfTypeAll<PlatformLeaderboardsModel>().Select(l => l._platformUserModel).LastOrDefault(p => p != null);

            UserInfo userInfo = await platformUserModel.GetUserInfo(CancellationToken.None);
            var tokenProvider = new PlatformAuthenticationTokenProvider(platformUserModel, userInfo);

            return Platform switch {
                AuthPlatform.Steam => (await tokenProvider.GetAuthenticationToken()).sessionToken,
                AuthPlatform.OculusPC => (await tokenProvider.GetXPlatformAccessToken(CancellationToken.None)).token,
                _ => throw new ArgumentOutOfRangeException()
            };
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

        private static IEnumerator DoLogin(Action onSuccess, Action<string> onFail, int count = 1) {
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
                new MultipartFormDataSection("ticket", authToken),
                new MultipartFormDataSection("provider", provider),
                new MultipartFormDataSection("returnUrl", "/")
            };

            var request = UnityWebRequest.Post(BLConstants.SIGNIN_WITH_TICKET, form);
            
            yield return request.SendWebRequest();

            switch (request.responseCode) {
                case 200:
                    Plugin.Log.Info("Login successful!");
                    onSuccess();
                    var cookies = request.GetResponseHeader("Set-Cookie");
                    SetAuthCookie(cookies);
                    break;
                case BLConstants.MaintenanceStatus:
                    Plugin.Log.Debug("Login failed! Maintenance");
                    onFail("Maintenance");
                    break;
                default:
                    if (count == 4) {
                        Plugin.Log.Debug($"Login failed! status: {request.responseCode} error: {request.error}");
                        onFail($"NetworkError: {request.responseCode}");
                    } else {
                        Plugin.Log.Debug($"Retrying login #{count}! status: {request.responseCode} error: {request.error}");
                        yield return DoLogin(onSuccess, onFail, count++);
                    }
                    break;
            }
        }

        private static void SetAuthCookie(string cookies) {
            var cookieName = ".AspNetCore.Cookies";
            var value = ParseValue(cookies, cookieName);
            var domain = ParseValue(cookies, "domain");
            var cookie = new Cookie(cookieName, value, "/", domain);
            WebRequestFactory.CookieContainer.Add(cookie);
            return;

            static string ParseValue(string cookie, string param) {
                param += "=";
                var entry = cookie.IndexOf(param, StringComparison.Ordinal);
                cookie = cookie.Remove(0, entry + param.Length);
                //
                var exit = cookie.IndexOf(";", StringComparison.Ordinal);
                return cookie.Remove(exit, cookie.Length - exit);
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