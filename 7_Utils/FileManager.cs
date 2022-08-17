using BeatLeader.Models;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BeatLeader.Utils
{
    internal static class FileManager
    {
        #region Directories

        private static readonly string ReplaysFolderPath = Environment.CurrentDirectory + "\\UserData\\BeatLeader\\Replays\\";
        private static readonly string PlaylistsFolderPath = Environment.CurrentDirectory + "\\Playlists\\";

        static FileManager()
        {
            EnsureDirectoryExists(ReplaysFolderPath);
            EnsureDirectoryExists(PlaylistsFolderPath);
        }

        public static void EnsureDirectoryExists(string directory) {
            var path = Path.GetDirectoryName(directory);
            if (Directory.Exists(path) || path == null) return;
            Directory.CreateDirectory(path);
        }

        #endregion

        #region Replay

        public static void WriteReplay(Replay replay)
        {
            try {
                string filename = ToFileName(replay);
                using BinaryWriter file = new(File.Open(filename, FileMode.OpenOrCreate), Encoding.UTF8);
                ReplayEncoder.Encode(replay, file);
                file.Close();
            } catch (Exception ex) {
                Plugin.Log.Debug($"Unable to save replay. Reason: {ex.Message}");
            }
        }

        public static string ToFileName(Replay replay)
        {
            string practice = replay.info.speed != 0 ? "-practice" : "";
            string fail = replay.info.failTime != 0 ? "-fail" : "";
            string filename = $"{replay.info.playerID}{practice}{fail}-{replay.info.songName}-{replay.info.difficulty}-{replay.info.mode}-{replay.info.hash}.bsor";
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new (string.Format("[{0}]", Regex.Escape(regexSearch)));
            return ReplaysFolderPath + r.Replace(filename, "_");
        }

        public static Replay ReadReplay(string filename)
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

                    return ReplayDecoder.Decode(buffer);
                }
            }
            catch (Exception e)
            {
                Plugin.Log.Debug(e);
            }
            return null;
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
