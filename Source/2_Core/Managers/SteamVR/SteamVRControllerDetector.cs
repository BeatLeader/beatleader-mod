using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BeatLeader.SteamVR {
    internal static class SteamVRControllerDetector {
        #region Detect

        public static Task<string?> DetectAsync() {
            return Task.Run(Detect);
        }

        private static string? Detect() {
            try {
                var logDirectory = SteamVRFiles.LogDirectory;
                if (logDirectory == null) return null;

                using var process = Process.GetCurrentProcess();
                var session = new Session(process.Id.ToString(), GetStartTime(process));

                foreach (var fileName in logFiles) {
                    var path = Path.Combine(logDirectory, fileName);
                    if (!File.Exists(path) || File.GetLastWriteTime(path) < session.StartTime) continue;

                    foreach (var line in SteamVRFiles.ReadLinesShared(path)) {
                        session.ProcessLine(line);
                    }
                }

                return session.ControllerType;
            } catch (Exception ex) {
                Plugin.Log.Debug($"SteamVR controller detection failed: {ex.Message}");
                return null;
            }
        }

        private static DateTime GetStartTime(Process process) {
            try {
                return process.StartTime.AddSeconds(-1);
            } catch (Exception) {
                return DateTime.MinValue;
            }
        }

        #endregion

        #region Session

        private static readonly string[] logFiles = { "vrserver.previous.txt", "vrserver.txt" };

        // Fri Sep 25 2026 00:24:14.352 [Info] - SetApplicationPid appkey=steam.app.620980 pid=22672, external transition
        // On Linux the fraction has 6 digits, only the milliseconds are parsed
        private static readonly Regex timestampRegex = new(@"^\w{3} \w{3} \d{1,2} \d{4} \d{2}:\d{2}:\d{2}\.\d{3}");
        private static readonly string[] timestampFormats = { "ddd MMM dd yyyy HH:mm:ss.fff", "ddd MMM d yyyy HH:mm:ss.fff" };
        private static readonly Regex appPidRegex = new(@"SetApplicationPid appkey=(?<key>.+?) pid=(?<pid>\d+)");

        // Fri Sep 25 2026 00:24:14.586 [Info] - [Input] steam.app.620980 (frame_controller) attempting to load default config from ...
        private static readonly Regex inputRegex = new(@"\[Input\] (?<key>.+?) \((?<type>[^()\s]+)\)");

        // Under Proton the game has a Wine PID while SteamVR logs the Linux one, so the Steam app key is used when no PID matches
        private const string SteamAppKey = "steam.app.620980";

        private class Session {
            public DateTime StartTime { get; }
            public string? ControllerType => _appKey != null ? _appKeyControllerType : _steamAppKeyControllerType;

            private readonly string _pid;
            private bool _started;
            private string? _appKey;
            private string? _appKeyControllerType;
            private string? _steamAppKeyControllerType;

            public Session(string pid, DateTime startTime) {
                _pid = pid;
                StartTime = startTime;
            }

            public void ProcessLine(string line) {
                if (!_started) {
                    if (!TryParseTimestamp(line, out var time) || time < StartTime) return;
                    _started = true;
                }

                if (line.Contains("SetApplicationPid")) {
                    var match = appPidRegex.Match(line);
                    if (match.Success && match.Groups["pid"].Value == _pid) {
                        _appKey = match.Groups["key"].Value;
                    }
                    return;
                }

                if (!line.Contains("[Input] ")) return;

                var inputMatch = inputRegex.Match(line);
                if (!inputMatch.Success) return;

                var type = inputMatch.Groups["type"].Value;
                if (!IsHandController(type)) return;

                var key = inputMatch.Groups["key"].Value;
                if (key == _appKey) _appKeyControllerType = type;
                if (key == SteamAppKey) _steamAppKeyControllerType = type;
            }
        }

        private static bool TryParseTimestamp(string line, out DateTime time) {
            time = default;
            var match = timestampRegex.Match(line);
            return match.Success && DateTime.TryParseExact(
                match.Value, timestampFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out time
            );
        }

        #endregion

        #region IsHandController

        private static readonly HashSet<string> headsetTypes = new() { "rift", "vive", "vive_pro" };
        private static readonly string[] nonHandTypeParts = { "hmd", "tracker", "gamepad" };

        private static bool IsHandController(string type) {
            return !headsetTypes.Contains(type) && !nonHandTypeParts.Any(type.Contains);
        }

        #endregion
    }
}
