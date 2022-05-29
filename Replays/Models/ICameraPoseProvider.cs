using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BeatLeader.Replays.Managers.InputManager;
using UnityEngine;

namespace BeatLeader.Replays
{
    public interface ICameraPoseProvider
    {
        string name { get; }
        bool updateEveryFrame { get; }
        bool injectAutomatically { get; }
        InputSystemType[] availableSystems { get; }

        Pose GetPose(Pose cameraPose);
    }
}
