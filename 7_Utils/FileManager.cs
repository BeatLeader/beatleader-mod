using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BeatLeader.Utils
{
    class FileManager
    {
        public static void WriteReplay(Replay replay)
        {
            var folderPath = Environment.CurrentDirectory + "\\UserData\\BeatLeader\\Replays\\";
            Directory.CreateDirectory(Path.GetDirectoryName(folderPath));

            string filename = replay.info.playerID + (replay.info.speed != 0 ? "-practice" : "") + (replay.info.failTime != 0 ? "-fail" : "") + "-" + replay.info.songName + "-" + replay.info.difficulty + "-" + replay.info.mode + "-" + replay.info.hash + ".bsor";
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            filename = folderPath + r.Replace(filename, "_");
            if (File.Exists(filename)) {
                Stream stream = File.Open(filename, FileMode.Open);
                int arrayLength = (int)stream.Length;
                byte[] buffer = new byte[arrayLength];
                stream.Read(buffer, 0, arrayLength);
                stream.Close();

                if (ReplayDecoder.Decode(buffer).info.score > replay.info.score) return;
            }
            using BinaryWriter file = new BinaryWriter(File.Open(filename, FileMode.OpenOrCreate), Encoding.UTF8);

            ReplayEncoder.Encode(replay, file);
            file.Close();
        }
    }
}
