using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Models {
    public interface ICameraController {
        IReadOnlyList<ICameraView> Views { get; }
        ICameraView SelectedView { get; }
        
        Camera Camera { get; }

        event Action<ICameraView>? CameraViewChangedEvent;

        void SetView(ICameraView view);
    }
}