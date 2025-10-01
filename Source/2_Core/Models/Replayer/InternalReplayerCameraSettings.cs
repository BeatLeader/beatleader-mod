using System.Collections.Generic;
using System.Linq;
using BeatLeader.Utils;
using Newtonsoft.Json;

namespace BeatLeader.Models {
    internal class InternalReplayerCameraSettings : ReplayerCameraSettings {
        public override string? CameraView {
            get => InputUtils.UsesFPFC ? FpfcCameraView : VRCameraView;
            set {
                if (InputUtils.UsesFPFC) {
                    FpfcCameraView = value;
                } else {
                    VRCameraView = value;
                }
            }
        }

        public override IReadOnlyList<ICameraView>? CameraViews {
            get => InputUtils.UsesFPFC ?
                FpfcCameraViews ??= DefaultFpfcViews.ToList() :
                VRCameraViews ??= DefaultVRViews.ToList();
            set {
                if (InputUtils.UsesFPFC) {
                    FpfcCameraViews = value?.ToList();
                } else {
                    VRCameraViews = value?.ToList();
                }
            }
        }

        public List<ICameraView>? VRCameraViews { get; set; }
        public List<ICameraView>? FpfcCameraViews { get; set; }

        public string? VRCameraView { get; set; }
        public string? FpfcCameraView { get; set; }
    }
}