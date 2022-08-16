using static BeatLeader.Replayer.Managers.InputManager;
using UnityEngine;

namespace BeatLeader.Models
{
    public interface ICameraPoseProvider
    {
        InputType[] AvailableInputs { get; }
        bool SelfInject { get; }
        bool UpdateEveryFrame { get; }
        string Name { get; }

        Pose GetPose(Pose cameraPose);
    }
}
