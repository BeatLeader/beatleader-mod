using BeatLeader.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace BeatLeader.Utils
{
    internal static class FileManager
    {
        #region Directories

        private static readonly string ReplaysFolderPath = Environment.CurrentDirectory + "\\UserData\\BeatLeader\\Replays\\";
        private static string CacheDirectory => Application.temporaryCachePath + "\\BeatLeader\\Replays\\";
        private static readonly string PlaylistsFolderPath = Environment.CurrentDirectory + "\\Playlists\\";

        public static string LastSavedReplay = "";

        static FileManager()
        {
            EnsureDirectoryExists(CacheDirectory);
            EnsureDirectoryExists(ReplaysFolderPath);
            EnsureDirectoryExists(PlaylistsFolderPath);
        }

        public static void EnsureDirectoryExists(string directory) {
            var path = Path.GetDirectoryName(directory);
            if (Directory.Exists(path) || path == null) return;
            Directory.CreateDirectory(path);
        }
        public static string[] GetAllReplaysPaths()
        {
            return Directory.GetFiles(ReplaysFolderPath);
        }
        public static bool TryWriteReplay(Replay replay) {
            LastSavedReplay = ToFileName(replay, ReplaysFolderPath);
            return TryWriteReplay(LastSavedReplay, replay);
        }
        public static bool TryWriteTempReplay(Replay replay)
        {
            LastSavedReplay = ToFileName(replay, CacheDirectory);
            return TryWriteReplay(LastSavedReplay, replay);
        }
        public static bool TryWriteReplay(string fileName, Replay replay)
        {
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

        public static string ToFileName(Replay replay, string folder)
        {
            string practice = replay.info.speed != 0 ? "-practice" : "";
            string fail = replay.info.failTime != 0 ? "-fail" : "";
            string filename = $"{replay.info.playerID}{practice}{fail}-{replay.info.songName}-{replay.info.difficulty}-{replay.info.mode}-{replay.info.hash}.bsor";
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new (string.Format("[{0}]", Regex.Escape(regexSearch)));
            return folder + r.Replace(filename, "_");
        }

        public static bool TryReadReplay(string filename, out Replay replay)
        {
            try
            {
                if (File.Exists(filename))
                {
                    Stream stream = File.Open(filename, FileMode.Open);
                    int arrayLength = (int)stream.Length;
                    byte[] buffer = new byte[arrayLength];
                    stream.Read(buffer, 0, arrayLength);
                    stream.Close();

                    replay = ReplayDecoder.Decode(buffer);
                    return true;
                }
            }
            catch (Exception e) {
                Plugin.Log.Debug(e);
            }
            
            replay = default;
            return false;
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
