using BeatLeader.Models;
using System;

namespace BeatLeader.Components {
    internal abstract class ParamsContentView<T> : ParamsContentViewBase where T : ICameraView {
        public override Type ViewType { get; } = typeof(T);
        protected T? View { get; private set; }

        public override void Setup(ICameraView poseProvider) {
            View = (T)poseProvider;
        }
    }
}
