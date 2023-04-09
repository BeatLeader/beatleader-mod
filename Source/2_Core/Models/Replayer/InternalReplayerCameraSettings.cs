using System.Collections.Generic;
using BeatLeader.Utils;
using IPA.Config.Stores.Attributes;

namespace BeatLeader.Models {
    internal class InternalReplayerCameraSettings : ReplayerCameraSettings {
        [Ignore]
        public override string? CameraView {
            get => InputUtils.IsInFPFC ? FpfcCameraView : VRCameraView;
            set {
                if (InputUtils.IsInFPFC) FpfcCameraView = value;
                else VRCameraView = value;
            }
        }

        [Ignore] 
        public override IReadOnlyList<ICameraView>? CameraViews {
            get => DefaultViews;
            set { }
        }

        public string? VRCameraView { get; set; }
        public string? FpfcCameraView { get; set; }
    }
}