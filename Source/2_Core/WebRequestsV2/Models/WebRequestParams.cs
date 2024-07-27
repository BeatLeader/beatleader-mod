namespace BeatLeader.WebRequests {
    public class WebRequestParams {
        public float UploadTrackingPrecision { get; set; } = 100f;
        public float DownloadTrackingPrecision { get; set; } = 100f;

        public float TrackingPrecision {
            set {
                UploadTrackingPrecision = value;
                DownloadTrackingPrecision = value;
            }
        }
    }
}