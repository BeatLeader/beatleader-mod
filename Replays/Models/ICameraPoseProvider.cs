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
        InputSystemType[] availableSystems { get; }
        bool selfInject { get; }
        bool updateEveryFrame { get; }
        string name { get; }

        Pose GetPose(Pose cameraPose);
    }
}
