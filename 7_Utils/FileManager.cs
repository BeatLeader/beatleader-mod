using BeatLeader.Models;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BeatLeader.Utils
{
    class FileManager
    {
        private static readonly string replayFolderPath = Environment.CurrentDirectory + "\\UserData\\BeatLeader\\Replays\\";

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
            using BinaryWriter file = new(File.Open(filename, FileMode.OpenOrCreate), Encoding.UTF8);
            ReplayEncoder.Encode(replay, file);
            file.Close();
        }

        public static string ToFileName(Replay replay)
        {
            string practice = replay.info.speed != 0 ? "-practice" : "";
            string fail = replay.info.failTime != 0 ? "-fail" : "";
            string filename = $"{replay.info.playerID}{practice}{fail}-{replay.info.songName}-{replay.info.difficulty}-{replay.info.mode}-{replay.info.hash}.bsor";
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new (string.Format("[{0}]", Regex.Escape(regexSearch)));
            return replayFolderPath + r.Replace(filename, "_");
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
    }
}
