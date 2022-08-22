using BeatLeader.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeatLeader.Components.Settings
{
    internal abstract class CameraParamsMenu : Menu
    {
        public abstract int Id { get; }
        public abstract Type Type { get; }
        public CameraPoseProvider PoseProvider { get; private set; }

        public void Init(CameraPoseProvider poseProvider)
        {
            PoseProvider = poseProvider;
            Handle();
        }

        protected override void OnInstantiate() { }
    }
}
