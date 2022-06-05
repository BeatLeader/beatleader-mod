using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BeatLeader.Replays.Emulating
{
    public class RescoreInvoker
    {
        public event Action<float, float> onRescoreRequested;

        public void Rescore(float startTime, float endTime)
        {
            onRescoreRequested?.Invoke(startTime, endTime);
        }
    }
}
