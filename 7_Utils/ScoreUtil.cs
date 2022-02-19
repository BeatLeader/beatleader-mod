using System;
using BeatLeader.Models;

namespace BeatLeader.Utils
{
    class ScoreUtil
    {
        public static void ProcessReplay(Replay replay) 
        {
            if (replay.info.score <= 0) return; // no lightshow here

            bool practice = replay.info.speed != 0;
            bool fail = replay.info.failTime > 0;

            if (practice || fail) {
                FileManager.WriteReplay(replay); // save the last replay
                return;
            }

            var localReplay = FileManager.ReadReplay(FileManager.ToFileName(replay));
            int localHighScore = localReplay == null ? int.MinValue : localReplay.info.score;

            // int serverHighScore = TODO

            if (localHighScore < replay.info.score) {
                FileManager.WriteReplay(replay);
                UploadManager.UploadReplay(replay);
            }
        }
    }
}
