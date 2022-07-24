using static BeatLeader.Replays.Managers.InputManager;
using UnityEngine;

namespace BeatLeader.Models
{
    public interface ICameraPoseProvider
    {
        InputType[] availableInputs { get; }
        bool selfInject { get; }
        bool updateEveryFrame { get; }
        string name { get; }

        Pose GetPose(Pose cameraPose);
    }
}
