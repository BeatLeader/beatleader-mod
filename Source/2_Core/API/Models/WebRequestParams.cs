using System;

namespace BeatLeader.WebRequests {
    public class WebRequestParams {
        public float UploadTrackingPrecision { get; set; } = 100f;
        public float DownloadTrackingPrecision { get; set; } = 100f;
        public int TimeoutSeconds { get; set; } = 30;
        public int RetryCount { get; set; } = 0;

        public float TrackingPrecision {
            set {
                UploadTrackingPrecision = value;
                DownloadTrackingPrecision = value;
            }
        }
    }
}