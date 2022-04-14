using BeatLeader.Replays.Models;
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using BeatLeader.Utils.Json;

namespace BeatLeader.Utils
{
    class FileManager
    {
        public static string replayFolderPath => @"UserData\BeatLeader\Replays\";

        static FileManager()
        {
            if (!Directory.Exists(Path.GetDirectoryName(replayFolderPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(replayFolderPath));
            }
        }

        public static void WriteReplay(Replay replay)
        {
            string filename = ToFileName(replay);
            File.WriteAllText($@"C:\Users\{Environment.UserName}\Desktop\replay.bsor", JsonUtil.ToJson(replay));
        }

        public static string ToFileName(Replay replay)
        {
            string practice = replay.info.speed != 0 ? "-practice" : "";
            string fail = replay.info.failTime != 0 ? "-fail" : "";
            string filename = $"{replay.info.playerID}{practice}{fail}-{replay.info.songName}-{replay.info.difficulty}-{replay.info.mode}-{replay.info.hash}.bsor";
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return replayFolderPath + r.Replace(filename, "_");
        }

        public static Replay ReadReplay(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                Stream stream = File.Open(path, FileMode.Open);
                int arrayLength = (int)stream.Length;
                byte[] buffer = new byte[arrayLength];
                stream.Read(buffer, 0, arrayLength);
                stream.Close();

                return null;//ReplayDecoder.Decode(buffer);
            }
            catch (Exception e)
            {
                Plugin.Log.Debug(e);
            }
            return null;
        }
    }
}
