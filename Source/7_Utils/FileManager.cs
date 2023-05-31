using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BeatLeader.Models.Replay;
using UnityEngine;

namespace BeatLeader.Utils {
    internal static class FileManager {
        #region Replays

        public static IEnumerable<string> GetAllReplayPaths() {
            return Directory.EnumerateFiles(ReplaysFolderPath, "*.bsor");
        }

        public static bool TryWriteReplay(Replay replay) {
            LastSavedReplay = ToFileName(replay, ReplaysFolderPath);
            return TryWriteReplay(LastSavedReplay, replay);
        }

        public static bool TryWriteTempReplay(Replay replay) {
            LastSavedReplay = ToFileName(replay, CacheDirectory);
            return TryWriteReplay(LastSavedReplay, replay);
        }

        public static bool TryWriteReplay(string fileName, Replay replay) {
            try {
                EnsureDirectoryExists(fileName);
                using BinaryWriter file = new(File.Open(fileName, FileMode.OpenOrCreate), Encoding.UTF8);
                ReplayEncoder.Encode(replay, file);
                file.Close();
                return true;
            } catch (Exception ex) {
                Plugin.Log.Debug($"Unable to save replay. Reason: {ex.Message}");
                return false;
            }
        }

        public static string ToFileName(Replay replay, string folder) {
            var practice = replay.info.speed != 0 ? "-practice" : "";
            var fail = replay.info.failTime != 0 ? "-fail" : "";
            var filename = $"{replay.info.playerID}{practice}{fail}-{replay.info.songName}-{replay.info.difficulty}-{replay.info.mode}-{replay.info.hash}.bsor";
            var regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            var r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return folder + r.Replace(filename, "_");
        }

        public static bool TryReadReplay(string path, out Replay? replay) {
            if (File.Exists(path)) {
                return ReplayDecoder.TryDecodeReplay(File.ReadAllBytes(path), out replay);
            }
            replay = null;
            return false;
        }

        public static bool TryReadReplayInfo(string path, out ReplayInfo? replayInfo) {
            if (File.Exists(path)) {
                return ReplayDecoder.TryDecodeReplayInfo(File.ReadAllBytes(path), out replayInfo);
            }
            replayInfo = null;
            return false;
        }

        #endregion

        #region Directories

        private static readonly string ReplaysFolderPath = Environment.CurrentDirectory + "\\UserData\\BeatLeader\\Replays\\";
        private static string CacheDirectory => Application.temporaryCachePath + "\\BeatLeader\\Replays\\";
        private static readonly string PlaylistsFolderPath = Environment.CurrentDirectory + "\\Playlists\\";

        public static string LastSavedReplay = "";

        static FileManager() {
            EnsureDirectoryExists(CacheDirectory);
            EnsureDirectoryExists(ReplaysFolderPath);
            EnsureDirectoryExists(PlaylistsFolderPath);
        }

        public static void EnsureDirectoryExists(string directory) {
            var path = Path.GetDirectoryName(directory);
            if (Directory.Exists(path) || path == null) return;
            Directory.CreateDirectory(path);
        }

        #endregion

        #region Playlists

        private static string GetPlaylistFileName(string name, bool json = false) => $"{PlaylistsFolderPath}{name}.{(json ? "json" : "bplist")}";

        public static void DeletePlaylist(string fileName) {
            try {
                var bplist = GetPlaylistFileName(fileName);
                var json = GetPlaylistFileName(fileName, true);
                if (File.Exists(bplist)) File.Delete(bplist);
                if (File.Exists(json)) File.Delete(json);
            } catch (Exception) {
                //Suppress
            }
        }

        public static bool TryReadPlaylist(string fileName, out byte[] bytes) {
            try {
                bytes = File.ReadAllBytes(GetPlaylistFileName(fileName));
                return true;
            } catch (Exception ex) {
                Plugin.Log.Debug($"Unable read playlist. Reason: {ex.Message}");
                bytes = Array.Empty<byte>();
                return false;
            }
        }

        public static bool TrySaveRankedPlaylist(string fileName, byte[] bytes) {
            try {
                using var writer = new BinaryWriter(File.Open(GetPlaylistFileName(fileName), FileMode.OpenOrCreate, FileAccess.Write));
                writer.Write(bytes);
                return true;
            } catch (Exception ex) {
                Plugin.Log.Debug($"Unable to write playlist. Reason: {ex.Message}");
                return false;
            }
        }

        #endregion
    }
}