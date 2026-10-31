using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BeatLeader.SteamVR {
    internal static class SteamVRHeadsetDetector {
        #region Detect

        public class Headset {
            public string Driver { get; }
            public string Serial { get; }
            public string? Manufacturer { get; set; }
            public string? Model { get; set; }

            public Headset(string driver, string serial) {
                Driver = driver;
                Serial = serial;
            }
        }

        public static Task<Headset?> DetectAsync() {
            return Task.Run(Detect);
        }

        private static Headset? Detect() {
            try {
                var logDirectory = SteamVRFiles.LogDirectory;
                if (logDirectory == null) return null;

                var parser = new Parser();
                foreach (var fileName in logFiles) {
                    var path = Path.Combine(logDirectory, fileName);
                    if (!File.Exists(path)) continue;

                    foreach (var line in SteamVRFiles.ReadLinesShared(path)) {
                        parser.ProcessLine(line);
                    }
                }

                return parser.ActiveHeadset;
            } catch (Exception ex) {
                Plugin.Log.Debug($"SteamVR headset detection failed: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Parser

        private static readonly string[] logFiles = { "vrserver.previous.txt", "vrserver.txt" };

        // Mon Oct 05 2026 12:41:25.409 [Info] - Active HMD set to vrlink.VRLINKHMDQUEST3
        // Mon Oct 05 2026 12:41:25.676 [Info] - Using existing HMD vrlink.VRLINKHMDQUEST3
        private static readonly Regex activeHeadsetRegex = new(@"\] - (?:Active HMD set to|Using existing HMD) (?<driver>[^.\s]+)\.(?<serial>.+)$");

        // Mon Oct 05 2026 12:41:25.110 [Info] - Driver 'vrlink' started activation of tracked device with serial number 'VRLINKHMDQUEST3'
        // Mon Oct 05 2026 12:41:25.410 [Info] - Driver 'vrlink' finished adding tracked device with serial number 'VRLINKHMDQUEST3'
        private static readonly Regex deviceRegex = new(@"\] - Driver '(?<driver>[^']+)' (?:started activation of|finished adding) tracked device");

        // Mon Oct 05 2026 12:41:25.405 [Info] - vrlink: SerialNumber VRLINKHMDQUEST3
        // Mon Oct 05 2026 12:41:25.405 [Info] - vrlink: ModelNumber Oculus Quest3
        private static readonly Regex propertyRegex = new(@"\] - (?<driver>[^:\s]+): (?<property>SerialNumber|ManufacturerName|ModelNumber) (?<value>.+)$");

        private class Parser {
            public Headset? ActiveHeadset => _activeKey != null && _headsets.TryGetValue(_activeKey, out var headset) ? headset : null;

            private readonly Dictionary<string, Headset> _headsets = new(StringComparer.OrdinalIgnoreCase);
            private readonly Dictionary<string, Headset> _propertiesTarget = new(StringComparer.OrdinalIgnoreCase);
            private string? _activeKey;

            public void ProcessLine(string line) {
                if (line.Contains("HMD ")) {
                    var match = activeHeadsetRegex.Match(line);
                    if (match.Success) {
                        var headset = GetHeadset(match.Groups["driver"].Value, match.Groups["serial"].Value.Trim());
                        _activeKey = Key(headset.Driver, headset.Serial);
                        return;
                    }
                }

                if (line.Contains("tracked device")) {
                    var match = deviceRegex.Match(line);
                    if (match.Success) _propertiesTarget.Remove(match.Groups["driver"].Value);
                    return;
                }

                var propertyMatch = propertyRegex.Match(line);
                if (!propertyMatch.Success) return;

                var driver = propertyMatch.Groups["driver"].Value;
                var value = propertyMatch.Groups["value"].Value.Trim();

                switch (propertyMatch.Groups["property"].Value) {
                    case "SerialNumber":
                        _propertiesTarget[driver] = GetHeadset(driver, value);
                        break;
                    case "ManufacturerName" when _propertiesTarget.TryGetValue(driver, out var target):
                        target.Manufacturer = value;
                        break;
                    case "ModelNumber" when _propertiesTarget.TryGetValue(driver, out var target) && !value.Contains("Controller"):
                        target.Model = value;
                        break;
                }
            }

            private Headset GetHeadset(string driver, string serial) {
                var key = Key(driver, serial);
                if (!_headsets.TryGetValue(key, out var headset)) {
                    headset = new Headset(driver, serial);
                    _headsets[key] = headset;
                }
                return headset;
            }

            private static string Key(string driver, string serial) => driver + "." + serial;
        }

        #endregion
    }
}
