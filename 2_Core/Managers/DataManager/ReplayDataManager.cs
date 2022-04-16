using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeatLeader.Models;
using BeatLeader.Utils;

namespace BeatLeader.DataManager
{
    public class ReplayDataManager
    {
        public static IDifficultyBeatmap lastBeatmapData;
        public static Replay lastReplay;

        public static Replay GetReplay()
        {
            ReplayDataHelper.TryGetReplayBySongInfo(lastBeatmapData, out Replay replay);
            lastReplay = replay;
            return replay; 
        }
    }
}
