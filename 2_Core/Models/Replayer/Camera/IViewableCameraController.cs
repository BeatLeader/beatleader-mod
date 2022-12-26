using System;
using System.Collections.Generic;
using UnityEngine;

namespace BeatLeader.Models {
    public interface IViewableCameraController : ICameraControllerBase {
        IList<ICameraView> Views { get; }
        ICameraView? SelectedView { get; }
        Camera? Camera { get; }

        event Action<ICameraView>? CameraViewChangedEvent;

        void SetCamera(Camera camera);
        bool SetView(string name);
        void SetEnabled(bool enabled = true);
    }
}
