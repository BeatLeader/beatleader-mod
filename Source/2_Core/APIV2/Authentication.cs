using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;
using BeatLeader.WebRequests;
using BS_Utils.Gameplay;
using IPA.Utilities.Async;
using UnityEngine;

namespace BeatLeader.APIV2 {
    internal static class Authentication {
        #region Platform

        public enum AuthPlatform {
            Undefined,
            Steam,
            OculusPC
        }

        public static AuthPlatform Platform { get; private set; }

        public static void SetPlatform(AuthPlatform platform) {
            Platform = platform;
        }

        public static Task<string?> PlatformTicket() {
            // platform APIs are accessible from the main thread only
            return UnityMainThreadTaskScheduler.Factory.StartNew(PlatformTicketInternal).Unwrap();
        }

        private static async Task<string?> PlatformTicketInternal() {
            await GetUserInfo.GetUserAsync();

            var platformUserModel = Resources
                .FindObjectsOfTypeAll<PlatformLeaderboardsModel>()
                .Select(l => l._platformUserModel)
                .Last(x => x != null);

            var userInfo = await platformUserModel.GetUserInfo(CancellationToken.None);
            var tokenProvider = new PlatformAuthenticationTokenProvider(platformUserModel, userInfo);

            return Platform switch {
                AuthPlatform.Steam    => (await tokenProvider.GetAuthenticationToken()).sessionToken,
                AuthPlatform.OculusPC => (await tokenProvider.GetXPlatformAccessToken(CancellationToken.None)).token,
                _                     => throw new ArgumentOutOfRangeException()
            };
        }

        #endregion

        #region Login

        private static TaskCompletionSource<bool> _taskSource = new();
        private static bool _signedIn;

        public static void ResetLogin() {
            WebRequestFactory.CookieContainer.SetCookies(new Uri(BLConstants.BEATLEADER_API_URL), "");
            _signedIn = false;
            _taskSource = new();
        }

        public static Task<bool> WaitLogin() {
            return _taskSource.Task;
        }

        public static async Task Login() {
            if (_signedIn) return;

            if (!TryGetPlatformProvider(Platform, out var provider)) {
                Plugin.Log.Debug("Login failed! Unknown platform");
                return;
            }

            var authToken = await PlatformTicket();
            if (authToken == null) {
                Plugin.Log.Debug("Login failed! No auth token");
                return;
            }

            var result = await AuthRequest.Send(authToken, provider!).Join();

            switch ((int)result.RequestStatusCode) {
                case 200:
                    Plugin.Log.Info("Login successful!");
                    ShareCookiesBetweenServers();
                    _signedIn = true;
                    _taskSource.SetResult(true);
                    break;
                
                case BLConstants.MaintenanceStatus:
                    Plugin.Log.Debug("Login failed! Maintenance");
                    break;
                
                default:
                    Plugin.Log.Debug($"Login failed! status: {result.RequestStatusCode} error: {result.FailReason}");
                    break;
            }
        }

        // The session cookie is bound to the host it was issued for,
        // so it is copied to the other servers to keep the session alive after a domain switch
        private static void ShareCookiesBetweenServers() {
            var container = WebRequestFactory.CookieContainer;
            var sourceUri = new Uri(BLConstants.BEATLEADER_API_URL);
            var cookies = container.GetCookies(sourceUri);

            foreach (var server in BeatLeaderServerUtils.ServerOptions) {
                var targetUri = new Uri(server.GetAPIUrl());
                if (targetUri.Host == sourceUri.Host) continue;

                foreach (Cookie cookie in cookies) {
                    container.Add(targetUri, new Cookie(cookie.Name, cookie.Value, cookie.Path) {
                        Secure = cookie.Secure,
                        HttpOnly = cookie.HttpOnly,
                        Expires = cookie.Expires
                    });
                }
            }
        }

        private static bool TryGetPlatformProvider(AuthPlatform platform, out string? provider) {
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