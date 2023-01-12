using BeatLeader.Models;
using System;

namespace BeatLeader.Components {
    internal abstract class ParamsContentViewBase : ContentView {
        public abstract Type ViewType { get; }

        public abstract void Setup(ICameraView view);
    }
}
