using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
                Directory.CreateDirectory(path);
                await ExtractFiles(archive, path);
                SongCore.Loader.Instance.RefreshSongs(false);
                return true;
            } catch (Exception ex) {
                Plugin.Log.Error("Failed to install beatmap:\n" + ex);
                return false;
            }
        }

        private static Task ExtractFiles(ZipArchive archive, string path)
        {
            return Task.Run(() =>
            {
                foreach (var entry in archive.Entries)
                {
                    var entryPath = Path.Combine(path, entry.FullName);
                    entry.ExtractToFile(entryPath, true);
                }
            });
        }
        
        #endregion
        
        #region Replays

        public static IEnumerable<string> GetAllReplayPaths() {
            return Directory.EnumerateFiles(replaysFolderPath, "*.bsor")
                .Concat(Directory.EnumerateFiles(ReplayerCache.CacheDirectory, "*.bsor"));
        }

        public static string GetAbsoluteReplayPath(string fileName) {
            return Path.Combine(replaysFolderPath, fileName);
        }
        
        public static bool TryWriteReplay(string fileName, Replay replay) {
            try {
                var path = GetAbsoluteReplayPath(fileName);
                var file = File.Open(path, FileMode.OpenOrCreate);

                using (var stream = new BinaryWriter(file, Encoding.UTF8)) {
                    await Task.Run(() => ReplayEncoder.Encode(replay, stream), token);
                }

                Plugin.Log.Debug("[FileManager] Replay saved");
                
                return true;
            } catch (Exception ex) {
                Plugin.Log.Error($"[FileManager] Failed to save replay: {ex.Message}");
                
                return false;
            }
        }

        public static async Task<Replay?> ReadReplayAsync(string path, CancellationToken token) {
            if (!File.Exists(path)) {
                return null;
            }

            var bytes = await Task.Run(() => File.ReadAllBytes(path), token);

            // Unlike for replay info, we create a separate task as it takes 
            // more time to decode the whole replay
            var replay = await Task.Run(
                () => {
                    ReplayDecoder.TryDecodeReplay(bytes, out var replay);
                    return replay;
                },
                token
            );

            return replay;
        }

        public static async Task<ReplayInfo?> ReadReplayInfoAsync(string path, CancellationToken token) {
            if (!File.Exists(path)) {
                return null;
            }

            // Here we load the whole file as we don't have neither a stop byte
            // nor an implementation that would use streams instead of a byte array.
            // Implementing such thing could lead to a significant performance boost
            // when caching for the first time or reloading the cache.

            // According to my tests, this way seems to be significantly faster when
            // loading lots of small files (exactly our case) as the runtime don't need to 
            // switch contexts: it simply borrows the whole thread until the task is finished
            var bytes = await Task.Run(() => File.ReadAllBytes(path), token);

            ReplayDecoder.TryDecodeReplayInfo(bytes, out var replayInfo);

            return replayInfo;
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

        private static string GetPlaylistFileName(string name, bool json = false) {
            return $"{playlistsFolderPath}{name}.{(json ? "json" : "bplist")}";
        }

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