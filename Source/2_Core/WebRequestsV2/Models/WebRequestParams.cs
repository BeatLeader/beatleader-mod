using System;

namespace BeatLeader.WebRequests {
    public class WebRequestParams {
        public float UploadTrackingPrecision { get; set; } = 100f;
        public float DownloadTrackingPrecision { get; set; } = 100f;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        public float TrackingPrecision {
            set {
                UploadTrackingPrecision = value;
                DownloadTrackingPrecision = value;
            }
        }
    }
}