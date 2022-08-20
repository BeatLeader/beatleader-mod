using static BeatLeader.Replayer.Managers.InputManager;
using UnityEngine;

namespace BeatLeader.Models
{
    public interface ICameraPoseProvider
    {
        InputType[] AvailableInputs { get; }
        bool SupportsOffset { get; }
        bool UpdateEveryFrame { get; }
        string Name { get; }

        Pose GetPose(Pose cameraPose);
    }
}
