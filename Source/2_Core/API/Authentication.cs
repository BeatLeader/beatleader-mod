using System;
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
        public static AuthPlatform Platform { get; private set; }

        public static void SetPlatform(AuthPlatform platform) {
            Platform = platform;
        }

        public enum AuthPlatform {
            Undefined,
            Steam,
            OculusPC
        }

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

        private static bool _signedIn;
        public static string? authError = null;

        public static void ResetLogin() {
            WebRequestFactory.CookieContainer.SetCookies(new Uri(BLConstants.BEATLEADER_API_URL), "");
            _signedIn = false;
        }

        private static TaskCompletionSource<bool> TaskSource = new TaskCompletionSource<bool>();
        public static Task<bool> WaitLogin() {
            return TaskSource.Task;
        }

        public static async Task LogIn() {
            if (_signedIn) return;

            if (!TryGetPlatformProvider(Platform, out var provider)) {
                Plugin.Log.Debug("Login failed! Unknown platform");
                authError = "Unknown platform";
                return;
            }

            var authToken = await PlatformTicket();

            if (authToken == null) {
                Plugin.Log.Debug("Login failed! No auth token");
                authError = "No auth token";
                return;
            }

            var form = new List<IMultipartFormSection> {
                new MultipartFormDataSection("ticket", authToken),
                new MultipartFormDataSection("provider", provider),
                new MultipartFormDataSection("returnUrl", "/")
            };
            
            var result = await AuthRequest.Send(authToken, provider).Join();

            switch ((int)result.RequestStatusCode) {
                case 200:
                    Plugin.Log.Info("Login successful!");
                    _signedIn = true;
                    TaskSource.SetResult(true);
                    break;
                case BLConstants.MaintenanceStatus:
                    Plugin.Log.Debug("Login failed! Maintenance");
                    authError = "Maintenance";
                    break;
                default:
                    Plugin.Log.Debug($"Login failed! status: {result.RequestStatusCode} error: {result.FailReason}");
                    authError = $"NetworkError: {result.RequestStatusCode}";
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
    }
}