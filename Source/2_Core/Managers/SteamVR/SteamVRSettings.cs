using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace BeatLeader.SteamVR {
    internal static class SteamVRSettings {
        #region Settings

        private static readonly Dictionary<string, string> settings = new Dictionary<string, string>();

        private static readonly NumberFormatInfo nf = new NumberFormatInfo() {
            NumberDecimalSeparator = "."
        };

        public static float GetFloatOrDefault(string key, float defaultValue = default) {
            if (!settings.ContainsKey(key)) return defaultValue;

            try {
                return float.Parse(settings[key], nf);
            } catch (Exception) {
                return defaultValue;
            }
        }

        public static string? GetString(string key) {
            if (!settings.ContainsKey(key)) return null;

            return settings[key];
        }

        #endregion

        #region Update

        private const int TimeoutSeconds = 15;

        public static void UpdateAsync() {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(1000 * TimeoutSeconds);
            Task.Run(() => UpdateTask(cts.Token), cts.Token);
        }

        private static async Task UpdateTask(CancellationToken cancellationToken) {
            //<- Connect --------------------------------------------------
            var session = new SteamVRWebConsoleSession();
            var (state, failReason) = await session.ConnectTask(cancellationToken);

            if (state is not SteamVRWebConsoleSession.State.Opened) {
                Plugin.Log.Debug($"SteamVR console connection failed: {failReason}");
                return;
            }

            //<- Send 'settings' command ----------------------------------
            if (!session.TrySendCommandRequest("settings", out failReason)) {
                Plugin.Log.Debug($"SteamVR console settings request failed: {failReason}");
                return;
            }

            //<- Process response messages until timeout ------------------
            await session.ListenTask(ProcessMessage, cancellationToken);
            Plugin.Log.Debug($"SteamVR settings received: {settings.Count}");
        }

        #endregion

        #region ProcessMessages

        private enum ParseState {
            SearchForSettingsBlock,
            SkipFirstDashes,
            ParseSettings
        }

        private static ParseState _state = ParseState.SearchForSettingsBlock;

        private static void ProcessMessage(SteamVRWebConsoleSession.Message message) {
            var line = message.sMessage;

            var contentStart = line.IndexOf("[Console]", StringComparison.Ordinal);
            if (contentStart < 0) return;
            var content = line.Substring(contentStart + 10).TrimEnd();

            switch (_state) {
                case ParseState.SearchForSettingsBlock:
                    if (content.StartsWith("Settings:")) _state = ParseState.SkipFirstDashes;
                    break;

                case ParseState.SkipFirstDashes:
                    if (content.StartsWith("--")) _state = ParseState.ParseSettings;
                    break;

                case ParseState.ParseSettings:
                    if (content.StartsWith("--")) {
                        _state = ParseState.SearchForSettingsBlock;
                        break;
                    }

                    var separatorIndex = content.IndexOf(": ", StringComparison.Ordinal);
                    if (separatorIndex < 0) return;

                    var key = content.Substring(0, separatorIndex);
                    var value = content.Substring(separatorIndex + 2);
                    settings[key] = value;
                    break;
            }
        }

        #endregion
    }
}