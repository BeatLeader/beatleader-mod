using BeatLeader.Models;
using System;

namespace BeatLeader.Components.Settings
{
    internal abstract class CameraParamsMenu : Menu
    {
        public abstract int Id { get; }
        public abstract Type Type { get; }
        public ICameraPoseProvider PoseProvider { get; private set; }

        public void Init(ICameraPoseProvider poseProvider)
        {
            PoseProvider = poseProvider;
            Handle();
        }

        protected override void OnInstantiate() { }
    }
}
