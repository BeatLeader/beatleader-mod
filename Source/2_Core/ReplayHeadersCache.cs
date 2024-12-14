using System;
using System.Collections.Generic;
using System.IO;
using BeatLeader.Models;
using IPA.Utilities;
using Newtonsoft.Json;

namespace BeatLeader {
    internal static class ReplayHeadersCache {
        static ReplayHeadersCache() {
            try {
                if (!File.Exists(cacheFile)) {
                    infosDictionary = new();
                    return;
                }
                var content = File.ReadAllText(cacheFile);
                infosDictionary = JsonConvert.DeserializeObject<Dictionary<string, SerializableReplayInfo>>(content);
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to initialize {nameof(ReplayHeadersCache)}\n{ex}");
            }
            infosDictionary ??= new();
        }

        private static readonly string cacheFile = Path.Combine(UnityGame.UserDataPath, "BeatLeader", "ReplayHeadersCache");
        private static readonly Dictionary<string, SerializableReplayInfo> infosDictionary;

        public static bool TryGetInfoByPath(string path, out IReplayInfo? info) {
            if (!infosDictionary.TryGetValue(Path.GetFileName(path), out var serInfo)) {
                info = null;
                return false;
            }
            info = serInfo;
            return true;
        }

        public static void AddInfoByPath(string path, IReplayInfo info) {
            infosDictionary[Path.GetFileName(path)] = ToSerializableReplayInfo(info);
        }

        public static void RemoveInfoByPath(string path) {
            infosDictionary.Remove(Path.GetFileName(path));
        }

        public static void SaveCache() {
            try {
                var ser = JsonConvert.SerializeObject(infosDictionary);
                File.WriteAllText(cacheFile, ser);
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to save {nameof(ReplayHeadersCache)}\n{ex}");
            }
        }

        private static SerializableReplayInfo ToSerializableReplayInfo(IReplayInfo info) {
            return new() {
                FailTime = info.FailTime,
                LevelEndType = info.LevelEndType,
                PlayerID = info.PlayerID,
                PlayerName = info.PlayerName,
                SongDifficulty = info.SongDifficulty,
                SongHash = info.SongHash,
                SongMode = info.SongMode,
                SongName = info.SongName,
                Modifiers = info.Modifiers,
                Timestamp = info.Timestamp
            };
        }
    }
}