using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace BeatLeader.SteamVR {
    internal static class SteamVRFiles {
        #region Paths

        private static readonly Lazy<JObject?> openVRPaths = new(ReadOpenVRPaths);

        public static string? LogDirectory => GetFirstPath("log");

        private static string? GetFirstPath(string key) {
            var path = (openVRPaths.Value?[key] as JArray)?.FirstOrDefault()?.ToString();
            return path == null ? null : ToWindowsPath(path);
        }

        private static JObject? ReadOpenVRPaths() {
            try {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "openvr", "openvrpaths.vrpath");
                return File.Exists(path) ? JObject.Parse(File.ReadAllText(path)) : null;
            } catch (Exception ex) {
                Plugin.Log.Debug($"Unable to read openvrpaths.vrpath: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region ToWindowsPath

        // Under Proton the paths are written in Wine's native form of Unix paths (\??\unix/home/...),
        // which .NET can't open. Proton maps the Unix root to Z:
        private const string WineUnixPrefix = @"\??\unix";

        private static string ToWindowsPath(string path) {
            if (!path.StartsWith(WineUnixPrefix, StringComparison.Ordinal)) return path;
            return "Z:" + path.Substring(WineUnixPrefix.Length).Replace('/', '\\');
        }

        #endregion

        #region ReadLinesShared

        // SteamVR keeps its logs open for writing, so they have to be read with a permissive share mode
        public static IEnumerable<string> ReadLinesShared(string path) {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using var reader = new StreamReader(stream);
            while (reader.ReadLine() is { } line) {
                yield return line;
            }
        }

        #endregion
    }
}
