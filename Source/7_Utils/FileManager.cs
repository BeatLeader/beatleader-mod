using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BeatLeader.Interop;
using BeatLeader.Models.Activity;
using BeatLeader.Models.BeatSaver;
using BeatLeader.Models.Replay;
using UnityEngine;
using BeatLeader.Replayer;

namespace BeatLeader.Utils {
    internal static class FileManager {
        #region Beatmaps

        private static string BeatmapsDirectory => Path.Combine(Application.dataPath, "CustomLevels");

        public static async Task<bool> InstallBeatmap(byte[] bytes, string folderName) {
            try {
                var path = Path.Combine(BeatmapsDirectory, folderName);
                using var memoryStream = new MemoryStream(bytes);
                using var archive = new ZipArchive(memoryStream);
                foreach (var entry in archive.Entries) {
                    using var entryStream = entry.Open();
                    var streamLength = entry.Length;
                    var entryBuffer = new byte[streamLength];
                    var bytesRead = await entryStream.ReadAsync(entryBuffer, 0, (int)streamLength);
                    if (bytesRead < streamLength) throw new FileLoadException();
                    var destinationPath = Path.Combine(path, entry.FullName);
                    Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                    using var destinationStream = File.OpenWrite(destinationPath);
                    await destinationStream.WriteAsync(entryBuffer, 0, (int)streamLength);
                }
                SongCoreInterop.TryRefreshSongs(true);
                return true;
            } catch (Exception ex) {
                Plugin.Log.Error("Failed to install beatmap:\n" + ex);
                return false;
            }
        }
        
        #endregion
        
        #region Replays

        public static IEnumerable<string> GetAllReplayPaths() {
            return Directory.EnumerateFiles(replaysFolderPath, "*.bsor")
                .Concat(Directory.EnumerateFiles(ReplayerCache.CacheDirectory, "*.bsor"));
        }

        public static bool TryWriteReplay(string fileName, Replay replay) {
            try {
                EnsureDirectoryExists(fileName);
                using BinaryWriter file = new(File.Open(fileName, FileMode.OpenOrCreate), Encoding.UTF8);
                ReplayEncoder.Encode(replay, file);
                file.Close();
                Plugin.Log.Debug("Saved.");
                return true;
            } catch (Exception ex) {
                Plugin.Log.Error($"Unable to save replay. Reason: {ex.Message}");
                return false;
            }
        }

        public static string ToFileName(Replay replay, PlayEndData? playEndData) {
            return ToFileName(replay, playEndData, replaysFolderPath);
        }
        
        public static string ToFileName(Replay replay, PlayEndData? playEndData, string folder) {
            var practice = replay.info.speed != 0 ? "-practice" : "";
            var fail = replay.info.failTime != 0 ? "-fail" : "";
            var exit = playEndData?.EndType is PlayEndData.LevelEndType
                .Quit or PlayEndData.LevelEndType.Restart ? "-exit" : "";
            var info = replay.info;
            var filename = $"{info.playerID}{practice}{fail}{exit}-{info.songName}-{info.difficulty}-{info.mode}-{info.hash}-{info.timestamp}.bsor";
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
        
        private static readonly string replaysFolderPath = Environment.CurrentDirectory + "\\UserData\\BeatLeader\\Replays\\";
        private static readonly string playlistsFolderPath = Environment.CurrentDirectory + "\\Playlists\\";

        static FileManager() {
            EnsureDirectoryExists(replaysFolderPath);
            EnsureDirectoryExists(playlistsFolderPath);
            EnsureDirectoryExists(ReplayerCache.CacheDirectory);
        }

        public static void EnsureDirectoryExists(string directory) {
            var path = Path.GetDirectoryName(directory);
            if (Directory.Exists(path) || path == null) return;
            Directory.CreateDirectory(path);
        }

        #endregion

        #region Playlists

        private static string GetPlaylistFileName(string name, bool json = false) => $"{playlistsFolderPath}{name}.{(json ? "json" : "bplist")}";

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