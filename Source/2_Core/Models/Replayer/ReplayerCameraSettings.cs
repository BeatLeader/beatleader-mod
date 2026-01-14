using System.Collections.Generic;
using System.Linq;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using JetBrains.Annotations;
using Newtonsoft.Json;
using UnityEngine;

namespace BeatLeader.Models {
    [PublicAPI]
    public class ReplayerCameraSettings {
        public static readonly IReadOnlyList<ICameraView> DefaultFpfcViews = new List<ICameraView> {
            new StaticCameraView("LeftView", new(-3.70f, 1.70f, 0), new(0, 90, 0)),
            new StaticCameraView("RightView", new(3.70f, 1.70f, 0), new(0, -90, 0)),
            new StaticCameraView("BehindView", new(0f, 1.9f, -2f), Vector3.zero),
            new StaticCameraView("CenterView", new(0f, 1.7f, 0f), Vector3.zero),
            new PlayerViewCameraView {
                KeepStraight = true,
                Smoothness = 6f
            },
            new FlyingCameraView {
                Name = "FreeView",
                FlySpeed = 4,
                MouseSensitivity = new(0.5f, 0.5f),
                OriginPosition = new Vector3(0, 1.7f)
            }
        };

        public static readonly IReadOnlyList<ICameraView> DefaultVRViews = new List<ICameraView> {
            new StaticCameraView("LeftView", new(-3.70f, 0, -1.10f), new(0, 60, 0)),
            new StaticCameraView("RightView", new(3.70f, 0, -1.10f), new(0, -60, 0)),
            new StaticCameraView("BehindView", new(0, 0, -2), Vector3.zero),
            new StaticCameraView("CenterView", Vector3.zero, Vector3.zero),
            new ManualCameraView {
                Name = "CustomView"
            }
        };

        public static IReadOnlyList<ICameraView> DefaultViews => InputUtils.UsesFPFC ? DefaultFpfcViews : DefaultVRViews;

        public int MaxCameraFOV { get; set; }
        public int MinCameraFOV { get; set; }
        public int CameraFOV { get; set; }

        public string? CameraView {
            get => InputUtils.UsesFPFC ? FpfcCameraView : VRCameraView;
            set {
                if (InputUtils.UsesFPFC) {
                    FpfcCameraView = value;
                } else {
                    VRCameraView = value;
                }
            }
        }

        public IReadOnlyList<ICameraView> CameraViews {
            get => InputUtils.UsesFPFC ?
                FpfcCameraViews ??= DefaultFpfcViews.ToList() :
                VRCameraViews ??= DefaultVRViews.ToList();
            set {
                var views = value.ToList();
                
                if (InputUtils.UsesFPFC) {
                    FpfcCameraViews = views;
                } else {
                    VRCameraViews = views;
                }
            }
        }

#pragma warning disable MA0016
        public List<ICameraView>? VRCameraViews { get; set; }
        public List<ICameraView>? FpfcCameraViews { get; set; }
#pragma warning restore MA0016

        public string? VRCameraView { get; set; }
        public string? FpfcCameraView { get; set; }
    }
}