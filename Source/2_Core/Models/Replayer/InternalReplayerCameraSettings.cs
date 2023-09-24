using System.Collections.Generic;
using BeatLeader.Utils;

namespace BeatLeader.Models {
    internal class InternalReplayerCameraSettings : ReplayerCameraSettings {
        public override string? CameraView {
            get => InputUtils.IsInFPFC ? FpfcCameraView : VRCameraView;
            set {
                if (InputUtils.IsInFPFC) FpfcCameraView = value;
                else VRCameraView = value;
            }
        }
        
        public override IReadOnlyList<ICameraView>? CameraViews {
            get => DefaultViews;
            set { }
        }

        public string? VRCameraView { get; set; }
        public string? FpfcCameraView { get; set; }
    }
}