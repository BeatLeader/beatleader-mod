using BeatLeader.Models;
using System;

namespace BeatLeader.Components {
    internal abstract class ParamsContentView<T> : ParamsContentViewBase where T : ICameraPoseProvider {
        public override Type PoseType => _poseType;
        protected T Pose { get; private set; }

        private readonly Type _poseType = typeof(T);
        private bool _isPoseSet;

        public override void Setup(ICameraPoseProvider poseProvider) {
            if (_isPoseSet) return;
            Pose = (T)poseProvider;
            _isPoseSet = true;
        }
    }
}
