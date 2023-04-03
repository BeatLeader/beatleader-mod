using System.Collections.Generic;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using IPA.Config.Stores.Attributes;
using JetBrains.Annotations;
using UnityEngine;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerCameraSettings {
        public static readonly IReadOnlyList<ICameraView> DefaultFpfcViews = new List<ICameraView>() {
            new StaticCameraView("LeftView", new(-3.70f, 1.70f, 0), new(0, 90, 0)),
            new StaticCameraView("RightView", new(3.70f, 1.70f, 0), new(0, -90, 0)),
            new StaticCameraView("BehindView", new(0f, 1.9f, -2f), Vector3.zero),
            new StaticCameraView("CenterView", new(0f, 1.7f, 0f), Vector3.zero),
            new PlayerViewCameraView(),
            new FlyingCameraView("FreeView", new(0, 1.7f)) {
                mouseSensitivity = new(0.5f, 0.5f),
                flySpeed = 4
            },
        };

        public static readonly IReadOnlyList<ICameraView> DefaultVRViews = new List<ICameraView>() {
            new StaticCameraView("LeftView", new(-3.70f, 0, -1.10f), new(0, 60, 0)),
            new StaticCameraView("RightView", new(3.70f, 0, -1.10f), new(0, -60, 0)),
            new StaticCameraView("BehindView", new(0, 0, -2), Vector3.zero),
            new StaticCameraView("CenterView", Vector3.zero, Vector3.zero),
            new FlyingCameraView("CustomView") {
                mouseSensitivity = new(0.5f, 0.5f),
                flySpeed = 4
            },
        };

        public static IReadOnlyList<ICameraView> DefaultViews => InputUtils.IsInFPFC ? DefaultFpfcViews : DefaultVRViews;

        public int MaxCameraFOV { get; set; }
        public int MinCameraFOV { get; set; }
        public int CameraFOV { get; set; }
        [Ignore] public virtual string? CameraView { get; set; }
        [Ignore] public virtual IReadOnlyList<ICameraView>? CameraViews { get; set; }
    }
}