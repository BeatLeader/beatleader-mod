using BeatLeader.Models;
using System;

namespace BeatLeader.Components {
    internal abstract class ParamsContentViewBase : ContentView {
        public abstract Type PoseType { get; }
        public abstract int Id { get; }

        public abstract void Setup(ICameraPoseProvider provider);
    }
}
