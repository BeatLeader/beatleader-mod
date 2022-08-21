using static BeatLeader.Replayer.Managers.InputManager;
using UnityEngine;

namespace BeatLeader.Models
{
    public interface ICameraPoseProvider
    {
        InputType AvailableInputs { get; }
        int Id { get; }
        bool UpdateEveryFrame { get; }
        string Name { get; }

        Pose GetPose(Pose cameraPose);
    }
}
