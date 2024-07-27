using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Models {
    public interface ICameraController {
        IReadOnlyList<ICameraView> Views { get; }
        ICameraView SelectedView { get; }
        
        Camera Camera { get; }

        event Action<ICameraView>? CameraViewChangedEvent;
        event Action<int>? CameraFovChangedEvent;

        void SetFov(int fov);
        void SetView(ICameraView view);
    }
}