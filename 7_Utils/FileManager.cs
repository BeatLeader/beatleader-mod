using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Utils
{
    class FileManager
    {
        public static void WriteReplay(Replay replay)
        {
            var folderPath = Environment.CurrentDirectory + "\\UserData\\BeatLeader\\Replays\\";
            Directory.CreateDirectory(Path.GetDirectoryName(folderPath));

            using BinaryWriter file = new BinaryWriter(File.Open(folderPath + replay.info.playerID + (replay.info.speed != 0 ? "-practice" : "") + (replay.info.failTime != 0 ? "-fail" : "") + "-" + replay.info.songName + "-" + replay.info.mapper + "-" + replay.info.hash + ".bsor", FileMode.OpenOrCreate), Encoding.UTF8);

            ReplayEncoder.Encode(replay, file);
            file.Close();
        }
    }
}
