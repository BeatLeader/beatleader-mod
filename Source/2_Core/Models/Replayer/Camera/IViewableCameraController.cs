using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Models {
    //TODO: rework abstraction
    public interface IViewableCameraController : ICameraControllerBase {
        IList<ICameraView> Views { get; }
        ICameraView? SelectedView { get; }
        Camera? Camera { get; }

        event Action<ICameraView>? CameraViewChangedEvent;
        
        bool SetView(string name);
        void SetEnabled(bool enabled = true);
    }
}
