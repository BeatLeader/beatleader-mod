using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using BeatLeader.Replays.Models;

namespace BeatLeader.Replays
{
    public class ReplayJsonSerializer
    {
        public static string Serialize(Replay replay)
        {
            return JsonUtility.ToJson(replay);
        }
    }
}
